using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;

public sealed class OrganizationConfiguration :
    AggregateRoot<OrganizationConfigurationId>,
    IAuditableEntity
{
    private OrganizationConfiguration() { }

    private OrganizationConfiguration(
        OrganizationConfigurationId id,
        OrganizationId organizationId,
        int versionNumber,
        string countryCode,
        ConfigurationPayload payload)
        : base(id)
    {
        OrganizationId = organizationId;
        VersionNumber = versionNumber;
        CountryCode = countryCode;
        Payload = payload;
        Status = OrganizationConfigurationStatus.Draft;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public int VersionNumber { get; private set; }
    public string CountryCode { get; private set; } = string.Empty;
    public ConfigurationPayload Payload { get; private set; } = null!;
    public OrganizationConfigurationStatus Status { get; private set; }
    public DateTimeOffset? EffectiveFromUtc { get; private set; }
    public DateTimeOffset? EffectiveToUtc { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public UserId? PublishedByUserId { get; private set; }
    public int Revision { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<OrganizationConfiguration> CreateDraft(
        OrganizationConfigurationId id,
        OrganizationId organizationId,
        int versionNumber,
        string? countryCode,
        ConfigurationPayload payload)
    {
        if (id.IsEmpty)
            return Result.Failure<OrganizationConfiguration>(OrganizationConfigurationErrors.EmptyId);

        if (organizationId.IsEmpty)
            return Result.Failure<OrganizationConfiguration>(OrganizationConfigurationErrors.EmptyOrganizationId);

        if (versionNumber <= 0)
            return Result.Failure<OrganizationConfiguration>(OrganizationConfigurationErrors.InvalidVersion);

        string normalizedCountryCode = (countryCode ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedCountryCode.Length != 2 || normalizedCountryCode.Any(character => !char.IsLetter(character)))
            return Result.Failure<OrganizationConfiguration>(OrganizationConfigurationErrors.InvalidCountryCode);

        ArgumentNullException.ThrowIfNull(payload);

        var configuration = new OrganizationConfiguration(
            id,
            organizationId,
            versionNumber,
            normalizedCountryCode,
            payload);

        configuration.RaiseDomainEvent(new OrganizationConfigurationCreatedDomainEvent(
            configuration.Id,
            configuration.OrganizationId,
            configuration.VersionNumber));

        return Result.Success(configuration);
    }

    public Result UpdateDraft(ConfigurationPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (Status != OrganizationConfigurationStatus.Draft)
            return Result.Failure(OrganizationConfigurationErrors.DraftRequired);

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
        if (Status == OrganizationConfigurationStatus.Published)
            return Result.Failure(OrganizationConfigurationErrors.AlreadyPublished);

        if (Status != OrganizationConfigurationStatus.Draft)
            return Result.Failure(OrganizationConfigurationErrors.DraftRequired);

        if (effectiveToUtc.HasValue && effectiveToUtc.Value <= effectiveFromUtc)
            return Result.Failure(OrganizationConfigurationErrors.InvalidEffectivePeriod);

        if (publishedByUserId.IsEmpty)
            return Result.Failure(OrganizationConfigurationErrors.EmptyOrganizationId);

        EffectiveFromUtc = effectiveFromUtc;
        EffectiveToUtc = effectiveToUtc;
        PublishedAtUtc = publishedAtUtc;
        PublishedByUserId = publishedByUserId;
        Status = OrganizationConfigurationStatus.Published;
        Revision++;

        RaiseDomainEvent(new OrganizationConfigurationPublishedDomainEvent(
            Id,
            OrganizationId,
            VersionNumber,
            effectiveFromUtc));

        return Result.Success();
    }

    public Result Archive()
    {
        if (Status != OrganizationConfigurationStatus.Published)
            return Result.Failure(OrganizationConfigurationErrors.PublishedRequired);

        Status = OrganizationConfigurationStatus.Archived;
        Revision++;

        RaiseDomainEvent(new OrganizationConfigurationArchivedDomainEvent(
            Id,
            OrganizationId,
            VersionNumber));

        return Result.Success();
    }

    public bool IsEffectiveAt(DateTimeOffset instantUtc) =>
        Status == OrganizationConfigurationStatus.Published &&
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
