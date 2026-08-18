using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Domain.TrainingContracts;

public enum TrainingContractSignatoryKind
{
    Student = 0,
    Guardian = 1,
    ProviderRepresentative = 2,
    FunderRepresentative = 3,
    Other = 4
}

public enum SignatoryAuthorityStatus
{
    Unverified = 0,
    Verified = 1,
    Rejected = 2
}

public enum TrainingContractSignatoryStatus
{
    Pending = 0,
    Ready = 1,
    Signed = 2,
    Rejected = 3,
    Cancelled = 4
}

public sealed class TrainingContractSignatory
{
    private TrainingContractSignatory() { }

    private TrainingContractSignatory(
        TrainingContractSignatoryId id,
        TrainingContractId contractId,
        TrainingContractSignatoryKind kind,
        PersonId personId,
        OrganizationId? representedOrganizationId,
        string displayName,
        int signingOrder,
        bool isRequired,
        string? authorityReference)
    {
        Id = id;
        ContractId = contractId;
        Kind = kind;
        PersonId = personId;
        RepresentedOrganizationId = representedOrganizationId;
        DisplayName = displayName;
        SigningOrder = signingOrder;
        IsRequired = isRequired;
        AuthorityReference = authorityReference;
        AuthorityStatus = kind == TrainingContractSignatoryKind.Student
            ? SignatoryAuthorityStatus.Verified
            : SignatoryAuthorityStatus.Unverified;
        Status = AuthorityStatus == SignatoryAuthorityStatus.Verified
            ? TrainingContractSignatoryStatus.Ready
            : TrainingContractSignatoryStatus.Pending;
    }

    public TrainingContractSignatoryId Id { get; private set; }
    public TrainingContractId ContractId { get; private set; }
    public TrainingContractSignatoryKind Kind { get; private set; }
    public PersonId PersonId { get; private set; }
    public OrganizationId? RepresentedOrganizationId { get; private set; }
    public string DisplayName { get; private set; } = string.Empty;
    public int SigningOrder { get; private set; }
    public bool IsRequired { get; private set; }
    public string? AuthorityReference { get; private set; }
    public SignatoryAuthorityStatus AuthorityStatus { get; private set; }
    public UserId? AuthorityVerifiedByUserId { get; private set; }
    public DateTimeOffset? AuthorityVerifiedAtUtc { get; private set; }
    public string? AuthorityRejectionReason { get; private set; }
    public TrainingContractSignatoryStatus Status { get; private set; }

    internal static Result<TrainingContractSignatory> Create(
        TrainingContractId contractId,
        TrainingContractSignatoryKind kind,
        PersonId personId,
        OrganizationId? representedOrganizationId,
        string displayName,
        int signingOrder,
        bool isRequired,
        string? authorityReference)
    {
        if (contractId.IsEmpty || personId.IsEmpty || signingOrder < 1)
            return Result.Failure<TrainingContractSignatory>(TrainingContractErrors.InvalidSignatory);

        string normalizedName = displayName?.Trim() ?? string.Empty;
        if (normalizedName.Length is < 2 or > 250)
            return Result.Failure<TrainingContractSignatory>(TrainingContractErrors.InvalidSignatory);

        string? normalizedAuthority = Normalize(authorityReference);
        if (normalizedAuthority is { Length: > 250 })
            return Result.Failure<TrainingContractSignatory>(TrainingContractErrors.InvalidSignatory);

        if (kind == TrainingContractSignatoryKind.ProviderRepresentative && representedOrganizationId is null)
            return Result.Failure<TrainingContractSignatory>(TrainingContractErrors.SignatoryOrganizationRequired);

        return Result.Success(new TrainingContractSignatory(
            TrainingContractSignatoryId.New(), contractId, kind, personId, representedOrganizationId,
            normalizedName, signingOrder, isRequired, normalizedAuthority));
    }

    internal Result Update(int signingOrder, bool isRequired, string displayName, string? authorityReference)
    {
        if (signingOrder < 1) return Result.Failure(TrainingContractErrors.InvalidSignatory);
        string normalizedName = displayName?.Trim() ?? string.Empty;
        if (normalizedName.Length is < 2 or > 250) return Result.Failure(TrainingContractErrors.InvalidSignatory);
        string? normalizedAuthority = Normalize(authorityReference);
        if (normalizedAuthority is { Length: > 250 }) return Result.Failure(TrainingContractErrors.InvalidSignatory);

        SigningOrder = signingOrder;
        IsRequired = isRequired;
        DisplayName = normalizedName;
        AuthorityReference = normalizedAuthority;
        return Result.Success();
    }

    internal Result VerifyAuthority(UserId verifiedByUserId, DateTimeOffset verifiedAtUtc)
    {
        if (Status is TrainingContractSignatoryStatus.Signed or TrainingContractSignatoryStatus.Cancelled)
            return Result.Failure(TrainingContractErrors.SignatoryLocked);
        AuthorityStatus = SignatoryAuthorityStatus.Verified;
        AuthorityVerifiedByUserId = verifiedByUserId;
        AuthorityVerifiedAtUtc = verifiedAtUtc.ToUniversalTime();
        AuthorityRejectionReason = null;
        Status = TrainingContractSignatoryStatus.Ready;
        return Result.Success();
    }

    internal Result RejectAuthority(string reason, UserId verifiedByUserId, DateTimeOffset verifiedAtUtc)
    {
        string normalizedReason = reason?.Trim() ?? string.Empty;
        if (normalizedReason.Length is < 5 or > 500)
            return Result.Failure(TrainingContractErrors.InvalidAuthorityDecision);
        AuthorityStatus = SignatoryAuthorityStatus.Rejected;
        AuthorityVerifiedByUserId = verifiedByUserId;
        AuthorityVerifiedAtUtc = verifiedAtUtc.ToUniversalTime();
        AuthorityRejectionReason = normalizedReason;
        Status = TrainingContractSignatoryStatus.Pending;
        return Result.Success();
    }


    internal Result MarkSigned(DateTimeOffset signedAtUtc)
    {
        if (Status == TrainingContractSignatoryStatus.Signed)
            return Result.Success();
        if (Status != TrainingContractSignatoryStatus.Ready || AuthorityStatus != SignatoryAuthorityStatus.Verified)
            return Result.Failure(TrainingContractErrors.SignatoryNotReady);
        if (signedAtUtc == default)
            return Result.Failure(TrainingContractErrors.InvalidSignatureEvidence);

        Status = TrainingContractSignatoryStatus.Signed;
        return Result.Success();
    }

    private static string? Normalize(string? value)
    {
        string normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? null : normalized;
    }
}
