using DriveOS.Modules.Contracts.Domain.ContractAmendments.Events;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.ContractAmendments;

public sealed class ContractAmendment : AggregateRoot<ContractAmendmentId>, IAuditableEntity
{
    private ContractAmendment() { }

    private ContractAmendment(
        ContractAmendmentId id,
        OrganizationId organizationId,
        TrainingContractId contractId,
        int amendmentNumber,
        int baseContractVersionNumber,
        string reason,
        DateOnly effectiveDate,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot)
        : base(id)
    {
        OrganizationId = organizationId;
        ContractId = contractId;
        AmendmentNumber = amendmentNumber;
        BaseContractVersionNumber = baseContractVersionNumber;
        Reason = reason;
        EffectiveDate = effectiveDate;
        StartDate = startDate;
        EndDate = endDate;
        TotalAmount = totalAmount;
        Currency = currency;
        TermsSnapshot = termsSnapshot;
        Status = ContractAmendmentStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public TrainingContractId ContractId { get; private set; }
    public int AmendmentNumber { get; private set; }
    public int BaseContractVersionNumber { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateOnly EffectiveDate { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public TrainingContractTermsSnapshot TermsSnapshot { get; private set; } = null!;
    public ContractAmendmentStatus Status { get; private set; }
    public string? SignedDocumentReference { get; private set; }
    public string? SignedDocumentSha256 { get; private set; }
    public DateTimeOffset? SignedAtUtc { get; private set; }
    public UserId? SignatureRecordedByUserId { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }
    public UserId? AppliedByUserId { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public UserId? CancelledByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<ContractAmendment> CreateDraft(
        ContractAmendmentId id,
        OrganizationId organizationId,
        TrainingContractId contractId,
        int amendmentNumber,
        int baseContractVersionNumber,
        string reason,
        DateOnly effectiveDate,
        DateOnly startDate,
        DateOnly? endDate,
        decimal totalAmount,
        string currency,
        TrainingContractTermsSnapshot termsSnapshot)
    {
        if (id.IsEmpty) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidIdentifier);
        if (organizationId.IsEmpty || contractId.IsEmpty) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidOwner);
        if (amendmentNumber < 1) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidNumber);
        if (baseContractVersionNumber < 1) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidBaseVersion);
        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 10 or > 500) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidReason);
        if (effectiveDate == default) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidEffectiveDate);
        if (endDate is not null && endDate < startDate) return Result.Failure<ContractAmendment>(TrainingContractErrors.InvalidEffectivePeriod);
        if (totalAmount < 0 || totalAmount > 100_000_000m) return Result.Failure<ContractAmendment>(TrainingContractErrors.InvalidAmount);
        string normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(c => c is < 'A' or > 'Z')) return Result.Failure<ContractAmendment>(TrainingContractErrors.InvalidCurrency);
        if (termsSnapshot is null) return Result.Failure<ContractAmendment>(ContractAmendmentErrors.InvalidSnapshot);

        var amendment = new ContractAmendment(
            id, organizationId, contractId, amendmentNumber, baseContractVersionNumber,
            normalizedReason, effectiveDate, startDate, endDate, decimal.Round(totalAmount, 2), normalizedCurrency, termsSnapshot);

        amendment.RaiseDomainEvent(new ContractAmendmentDraftCreatedDomainEvent(
            id, contractId, organizationId, amendmentNumber, baseContractVersionNumber, effectiveDate));
        return Result.Success(amendment);
    }

    public Result MarkSigned(
        string signedDocumentReference,
        string documentSha256,
        UserId recordedByUserId,
        DateTimeOffset signedAtUtc)
    {
        if (Status != ContractAmendmentStatus.Draft) return Result.Failure(ContractAmendmentErrors.SignNotAllowed);
        if (string.IsNullOrWhiteSpace(signedDocumentReference) || string.IsNullOrWhiteSpace(documentSha256) || documentSha256.Trim().Length != 64 || recordedByUserId.IsEmpty || signedAtUtc == default)
            return Result.Failure(ContractAmendmentErrors.InvalidSignedDocument);

        SignedDocumentReference = signedDocumentReference.Trim();
        SignedDocumentSha256 = documentSha256.Trim().ToUpperInvariant();
        SignatureRecordedByUserId = recordedByUserId;
        SignedAtUtc = signedAtUtc.ToUniversalTime();
        Status = ContractAmendmentStatus.Signed;
        RaiseDomainEvent(new ContractAmendmentSignedDomainEvent(Id, ContractId, SignedDocumentSha256, recordedByUserId, SignedAtUtc.Value));
        return Result.Success();
    }

    public Result MarkApplied(int newContractVersionNumber, UserId appliedByUserId, DateTimeOffset appliedAtUtc)
    {
        if (Status != ContractAmendmentStatus.Signed || newContractVersionNumber <= BaseContractVersionNumber || appliedByUserId.IsEmpty || appliedAtUtc == default)
            return Result.Failure(ContractAmendmentErrors.ApplyNotAllowed);
        AppliedByUserId = appliedByUserId;
        AppliedAtUtc = appliedAtUtc.ToUniversalTime();
        Status = ContractAmendmentStatus.Applied;
        RaiseDomainEvent(new ContractAmendmentAppliedDomainEvent(Id, ContractId, newContractVersionNumber, appliedByUserId, AppliedAtUtc.Value));
        return Result.Success();
    }

    public Result Cancel(string reason, UserId cancelledByUserId, DateTimeOffset cancelledAtUtc)
    {
        if (Status is ContractAmendmentStatus.Applied or ContractAmendmentStatus.Cancelled) return Result.Failure(ContractAmendmentErrors.CancelNotAllowed);
        string normalized = reason?.Trim() ?? string.Empty;
        if (normalized.Length is < 5 or > 500) return Result.Failure(ContractAmendmentErrors.InvalidReason);
        CancellationReason = normalized;
        CancelledByUserId = cancelledByUserId;
        CancelledAtUtc = cancelledAtUtc.ToUniversalTime();
        Status = ContractAmendmentStatus.Cancelled;
        RaiseDomainEvent(new ContractAmendmentCancelledDomainEvent(Id, ContractId, normalized, cancelledByUserId, CancelledAtUtc.Value));
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}
