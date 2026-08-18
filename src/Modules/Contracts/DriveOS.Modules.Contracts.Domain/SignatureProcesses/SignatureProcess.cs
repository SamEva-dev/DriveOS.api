using DriveOS.Modules.Contracts.Domain.SignatureProcesses.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.SignatureProcesses;

public enum SignatureProcessStatus
{
    PendingDispatch = 0,
    InProgress = 1,
    PartiallySigned = 2,
    Completed = 3,
    Rejected = 4,
    Cancelled = 5,
    Expired = 6
}

public sealed record SignatureProcessRecipientSnapshot
{
    // Required by EF Core for owned-collection materialization.
    private SignatureProcessRecipientSnapshot() { }

    public SignatureProcessRecipientSnapshot(
        TrainingContractSignatoryId signatoryId,
        string kind,
        PersonId personId,
        OrganizationId? representedOrganizationId,
        string displayName,
        int signingOrder,
        bool isRequired)
    {
        SignatoryId = signatoryId;
        Kind = kind;
        PersonId = personId;
        RepresentedOrganizationId = representedOrganizationId;
        DisplayName = displayName;
        SigningOrder = signingOrder;
        IsRequired = isRequired;
    }

    public TrainingContractSignatoryId SignatoryId { get; private set; }
    public string Kind { get; private set; } = string.Empty;
    public PersonId PersonId { get; private set; }
    public OrganizationId? RepresentedOrganizationId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public int SigningOrder { get; private set; }
    public bool IsRequired { get; private set; }
}

public sealed class SignatureProcess : AggregateRoot<SignatureProcessId>
{
    private readonly List<SignatureProcessRecipientSnapshot> _recipients = [];
    private readonly List<SignatureEvidence> _evidence = [];

    private SignatureProcess() { }
    private SignatureProcess(SignatureProcessId id) : base(id) { }

    public OrganizationId OrganizationId { get; private set; }
    public TrainingContractId ContractId { get; private set; }
    public int ContractVersionNumber { get; private set; }
    public string DocumentReference { get; private set; } = string.Empty;
    public string DocumentSha256 { get; private set; } = string.Empty;
    public string SignatureOrder { get; private set; } = "Sequential";
    public SignatureProcessStatus Status { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public UserId RequestedByUserId { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public IReadOnlyCollection<SignatureProcessRecipientSnapshot> Recipients => _recipients.AsReadOnly();
    public IReadOnlyCollection<SignatureEvidence> Evidence => _evidence.AsReadOnly();

    public static Result<SignatureProcess> Create(
        SignatureProcessId id,
        OrganizationId organizationId,
        TrainingContractId contractId,
        int contractVersionNumber,
        string documentReference,
        string documentSha256,
        IEnumerable<SignatureProcessRecipientSnapshot> recipients,
        UserId requestedByUserId,
        DateTimeOffset requestedAtUtc)
    {
        var list = recipients?.OrderBy(x => x.SigningOrder).ToArray() ?? [];
        if (id.IsEmpty || organizationId.IsEmpty || contractId.IsEmpty || requestedByUserId.IsEmpty ||
            contractVersionNumber < 1 || string.IsNullOrWhiteSpace(documentReference) ||
            string.IsNullOrWhiteSpace(documentSha256) || documentSha256.Length != 64)
            return Result.Failure<SignatureProcess>(SignatureProcessErrors.InvalidRequest);

        if (list.Length == 0 || list.All(x => !x.IsRequired))
            return Result.Failure<SignatureProcess>(SignatureProcessErrors.RequiredSignatoryMissing);

        if (list.Any(x => x.SignatoryId.IsEmpty || x.PersonId.IsEmpty || x.SigningOrder < 1 || string.IsNullOrWhiteSpace(x.DisplayName)))
            return Result.Failure<SignatureProcess>(SignatureProcessErrors.InvalidRequest);

        if (list.GroupBy(x => x.SignatoryId).Any(x => x.Count() > 1))
            return Result.Failure<SignatureProcess>(SignatureProcessErrors.InvalidRequest);

        var process = new SignatureProcess(id)
        {
            OrganizationId = organizationId,
            ContractId = contractId,
            ContractVersionNumber = contractVersionNumber,
            DocumentReference = documentReference.Trim(),
            DocumentSha256 = documentSha256.Trim().ToUpperInvariant(),
            Status = SignatureProcessStatus.PendingDispatch,
            RequestedByUserId = requestedByUserId,
            RequestedAtUtc = requestedAtUtc.ToUniversalTime()
        };

        process._recipients.AddRange(list);
        return Result.Success(process);
    }

    public Result<SignatureEvidence> RecordSignature(
        TrainingContractSignatoryId signatoryId,
        string documentSha256,
        string signatureMethod,
        string authenticationMethod,
        string provider,
        string providerSignatureReference,
        string? certificateReference,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset signedAtUtc,
        DateTimeOffset receivedAtUtc,
        UserId recordedByUserId)
    {
        SignatureEvidence? existingByReference = _evidence.SingleOrDefault(x =>
            string.Equals(x.Provider, provider?.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.ProviderSignatureReference, providerSignatureReference?.Trim(), StringComparison.Ordinal));
        if (existingByReference is not null)
        {
            return existingByReference.SignatoryId == signatoryId
                ? Result.Success(existingByReference)
                : Result.Failure<SignatureEvidence>(SignatureProcessErrors.ProviderReferenceAlreadyUsed);
        }

        if (Status is SignatureProcessStatus.Completed or SignatureProcessStatus.Rejected or SignatureProcessStatus.Cancelled or SignatureProcessStatus.Expired)
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.ProcessLocked);

        SignatureProcessRecipientSnapshot? recipient = _recipients.SingleOrDefault(x => x.SignatoryId == signatoryId);
        if (recipient is null)
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.RecipientNotFound);

        if (!string.Equals(DocumentSha256, documentSha256?.Trim(), StringComparison.OrdinalIgnoreCase))
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.DocumentHashMismatch);

        if (_evidence.Any(x => x.SignatoryId == signatoryId))
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.SignatoryAlreadySigned);

        int nextRequiredOrder = _recipients
            .Where(x => x.IsRequired && _evidence.All(e => e.SignatoryId != x.SignatoryId))
            .Select(x => x.SigningOrder)
            .DefaultIfEmpty(recipient.SigningOrder)
            .Min();

        if (string.Equals(SignatureOrder, "Sequential", StringComparison.OrdinalIgnoreCase) && recipient.SigningOrder > nextRequiredOrder)
            return Result.Failure<SignatureEvidence>(SignatureProcessErrors.SignatureOrderViolation);

        Result<SignatureEvidence> evidenceResult = SignatureEvidence.Create(
            SignatureEvidenceId.New(), Id, recipient.SignatoryId, recipient.PersonId, DocumentSha256,
            signatureMethod, authenticationMethod, provider, providerSignatureReference,
            certificateReference, ipAddress, userAgent, signedAtUtc, receivedAtUtc, recordedByUserId);

        if (evidenceResult.IsFailure)
            return evidenceResult;

        _evidence.Add(evidenceResult.Value);
        Status = SignatureProcessStatus.InProgress;

        bool allRequiredSigned = _recipients
            .Where(x => x.IsRequired)
            .All(x => _evidence.Any(e => e.SignatoryId == x.SignatoryId));

        if (allRequiredSigned)
        {
            Status = SignatureProcessStatus.Completed;
            CompletedAtUtc = receivedAtUtc.ToUniversalTime();
        }
        else if (_evidence.Count > 0)
        {
            Status = SignatureProcessStatus.PartiallySigned;
        }

        RaiseDomainEvent(new ContractSignatureRecordedDomainEvent(
            Id, ContractId, signatoryId, evidenceResult.Value.Id, evidenceResult.Value.SignedAtUtc));

        if (allRequiredSigned)
        {
            RaiseDomainEvent(new ContractSignatureProcessCompletedDomainEvent(
                Id, ContractId, ContractVersionNumber, CompletedAtUtc!.Value));
        }

        return evidenceResult;
    }
}

public static class SignatureProcessErrors
{
    public static readonly Error InvalidRequest = Error.Validation(
        "Contracts.SignatureProcess.Invalid",
        "errors.contracts.signatureProcess.invalid");

    public static readonly Error RequiredSignatoryMissing = Error.Validation(
        "Contracts.SignatureProcess.RequiredSignatoryMissing",
        "errors.contracts.signatureProcess.requiredSignatoryMissing");

    public static readonly Error NotFound = Error.NotFound(
        "Contracts.SignatureProcess.NotFound",
        "errors.contracts.signatureProcess.notFound");

    public static readonly Error InvalidEvidence = Error.Validation(
        "Contracts.SignatureProcess.Evidence.Invalid",
        "errors.contracts.signatureProcess.evidence.invalid");

    public static readonly Error ProcessLocked = Error.Conflict(
        "Contracts.SignatureProcess.Locked",
        "errors.contracts.signatureProcess.locked");

    public static readonly Error RecipientNotFound = Error.NotFound(
        "Contracts.SignatureProcess.Recipient.NotFound",
        "errors.contracts.signatureProcess.recipient.notFound");

    public static readonly Error SignatoryAlreadySigned = Error.Conflict(
        "Contracts.SignatureProcess.Signatory.AlreadySigned",
        "errors.contracts.signatureProcess.signatory.alreadySigned");

    public static readonly Error ProviderReferenceAlreadyUsed = Error.Conflict(
        "Contracts.SignatureProcess.ProviderReference.AlreadyUsed",
        "errors.contracts.signatureProcess.providerReference.alreadyUsed");

    public static readonly Error DocumentHashMismatch = Error.Conflict(
        "Contracts.SignatureProcess.DocumentHash.Mismatch",
        "errors.contracts.signatureProcess.documentHash.mismatch");

    public static readonly Error SignatureOrderViolation = Error.Conflict(
        "Contracts.SignatureProcess.Order.Violation",
        "errors.contracts.signatureProcess.order.violation");
}

public interface ISignatureProcessRepository
{
    Task AddAsync(SignatureProcess process, CancellationToken cancellationToken = default);
    Task<SignatureProcess?> GetByIdAsync(SignatureProcessId id, CancellationToken cancellationToken = default);
    Task<bool> ExistsForContractVersionAsync(TrainingContractId contractId, int contractVersionNumber, CancellationToken cancellationToken = default);
}
