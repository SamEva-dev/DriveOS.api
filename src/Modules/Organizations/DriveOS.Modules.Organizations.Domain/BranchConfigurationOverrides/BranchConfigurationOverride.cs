using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides.Events;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;

public sealed class BranchConfigurationOverride :
    AggregateRoot<BranchConfigurationOverrideId>,
    IAuditableEntity
{
    private BranchConfigurationOverride() { }

    private BranchConfigurationOverride(
        BranchConfigurationOverrideId id,
        OrganizationId organizationId,
        BranchId branchId,
        OrganizationConfigurationId baseConfigurationId,
        int versionNumber,
        string countryCode,
        BranchOverridePayload payload)
        : base(id)
    {
        OrganizationId = organizationId;
        BranchId = branchId;
        BaseConfigurationId = baseConfigurationId;
        VersionNumber = versionNumber;
        CountryCode = countryCode;
        Payload = payload;
        Status = BranchConfigurationOverrideStatus.Draft;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BranchId BranchId { get; private set; }
    public OrganizationConfigurationId BaseConfigurationId { get; private set; }
    public int VersionNumber { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public BranchOverridePayload Payload { get; private set; } = null!;
    public BranchConfigurationOverrideStatus Status { get; private set; }
    public DateTimeOffset? EffectiveFromUtc { get; private set; }
    public DateTimeOffset? EffectiveToUtc { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public UserId? PublishedByUserId { get; private set; }
    public int Revision { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<BranchConfigurationOverride> CreateDraft(
        BranchConfigurationOverrideId id,
        OrganizationId organizationId,
        BranchId branchId,
        OrganizationConfigurationId baseConfigurationId,
        int versionNumber,
        string? countryCode,
        BranchOverridePayload payload)
    {
        if (id.IsEmpty)
            return Result.Failure<BranchConfigurationOverride>(BranchConfigurationOverrideErrors.EmptyId);
        if (organizationId.IsEmpty)
            return Result.Failure<BranchConfigurationOverride>(BranchConfigurationOverrideErrors.EmptyOrganizationId);
        if (branchId.IsEmpty)
            return Result.Failure<BranchConfigurationOverride>(BranchConfigurationOverrideErrors.EmptyBranchId);
        if (baseConfigurationId.IsEmpty)
            return Result.Failure<BranchConfigurationOverride>(BranchConfigurationOverrideErrors.EmptyBaseConfigurationId);
        if (versionNumber <= 0)
            return Result.Failure<BranchConfigurationOverride>(BranchConfigurationOverrideErrors.InvalidVersion);

        string normalizedCountryCode = (countryCode ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedCountryCode.Length != 2 || normalizedCountryCode.Any(character => !char.IsLetter(character)))
            return Result.Failure<BranchConfigurationOverride>(BranchConfigurationOverrideErrors.InvalidCountryCode);

        ArgumentNullException.ThrowIfNull(payload);

        var branchOverride = new BranchConfigurationOverride(
            id,
            organizationId,
            branchId,
            baseConfigurationId,
            versionNumber,
            normalizedCountryCode,
            payload);

        branchOverride.RaiseDomainEvent(new BranchConfigurationOverrideCreatedDomainEvent(
            branchOverride.Id,
            branchOverride.OrganizationId,
            branchOverride.BranchId,
            branchOverride.BaseConfigurationId,
            branchOverride.VersionNumber));

        return Result.Success(branchOverride);
    }

    public Result UpdateDraft(BranchOverridePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (Status != BranchConfigurationOverrideStatus.Draft)
            return Result.Failure(BranchConfigurationOverrideErrors.DraftRequired);

        if (Payload == payload)
            return Result.Success();

        Payload = payload;
        Revision++;
        return Result.Success();
    }

    public Result Publish(
        DateTimeOffset effectiveFromUtc,
        DateTimeOffset? effectiveToUtc,
        DateTimeOffset publishedAtUtc,
        UserId publishedByUserId)
    {
        if (Status == BranchConfigurationOverrideStatus.Published)
            return Result.Failure(BranchConfigurationOverrideErrors.AlreadyPublished);
        if (Status != BranchConfigurationOverrideStatus.Draft)
            return Result.Failure(BranchConfigurationOverrideErrors.DraftRequired);
        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= effectiveFromUtc)
            return Result.Failure(BranchConfigurationOverrideErrors.InvalidEffectivePeriod);
        if (publishedByUserId.IsEmpty)
            return Result.Failure(BranchConfigurationOverrideErrors.EmptyPublisher);

        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        PublishedAtUtc = publishedAtUtc;
        PublishedByUserId = publishedByUserId;
        Status = BranchConfigurationOverrideStatus.Published;
        Revision++;

        RaiseDomainEvent(new BranchConfigurationOverridePublishedDomainEvent(
            Id,
            OrganizationId,
            BranchId,
            VersionNumber,
            effectiveFromUtc));

        return Result.Success();
    }

    public Result Archive()
    {
        if (Status != BranchConfigurationOverrideStatus.Published)
            return Result.Failure(BranchConfigurationOverrideErrors.PublishedRequired);

        Status = BranchConfigurationOverrideStatus.Archived;
        Revision++;

        RaiseDomainEvent(new BranchConfigurationOverrideArchivedDomainEvent(
            Id,
            OrganizationId,
            BranchId,
            VersionNumber));

        return Result.Success();
    }

    public bool IsEffectiveAt(DateTimeOffset instantUtc) =>
        Status == BranchConfigurationOverrideStatus.Published &&
        EffectiveFromUtc.HasValue &&
        EffectiveFromUtc.Value <= instantUtc &&
        (!EffectiveToUtc.HasValue || instantUtc < EffectiveToUtc.Value);

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default) return;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }
}
