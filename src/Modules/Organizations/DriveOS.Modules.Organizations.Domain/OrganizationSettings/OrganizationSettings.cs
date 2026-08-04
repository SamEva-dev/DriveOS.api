using DriveOS.Modules.Organizations.Domain.OrganizationSettings.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public sealed class OrganizationSettings :
    AggregateRoot<OrganizationSettingsId>,
    IAuditableEntity
{
    private OrganizationSettings()
    {
    }

    private OrganizationSettings(
        OrganizationSettingsId id,
        OrganizationId organizationId,
        OrganizationProfile profile,
        OrganizationContactInformation contact,
        OrganizationAddress address,
        OrganizationRegionalSettings regional,
        OrganizationOperationalSettings operational)
        : base(id)
    {
        OrganizationId = organizationId;
        Profile = profile;
        Contact = contact;
        Address = address;
        Regional = regional;
        Operational = operational;
        Version = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public OrganizationProfile Profile { get; private set; } = null!;
    public OrganizationContactInformation Contact { get; private set; } = null!;
    public OrganizationAddress Address { get; private set; } = null!;
    public OrganizationRegionalSettings Regional { get; private set; } = null!;
    public OrganizationOperationalSettings Operational { get; private set; } = null!;
    public int Version { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<OrganizationSettings> Create(
        OrganizationSettingsId id,
        OrganizationId organizationId,
        OrganizationProfile profile,
        OrganizationContactInformation contact,
        OrganizationAddress address,
        OrganizationRegionalSettings regional,
        OrganizationOperationalSettings operational)
    {
        if (id.IsEmpty)
        {
            return Result.Failure<OrganizationSettings>(
                OrganizationSettingsErrors.EmptyId);
        }

        if (organizationId.IsEmpty)
        {
            return Result.Failure<OrganizationSettings>(
                OrganizationSettingsErrors.EmptyOrganizationId);
        }

        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(regional);
        ArgumentNullException.ThrowIfNull(operational);

        var settings = new OrganizationSettings(
            id,
            organizationId,
            profile,
            contact,
            address,
            regional,
            operational);

        settings.RaiseDomainEvent(
            new OrganizationSettingsCreatedDomainEvent(
                settings.Id,
                settings.OrganizationId,
                settings.Version));

        return Result.Success(settings);
    }

    public Result UpdateProfile(OrganizationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (Profile == profile)
        {
            return Result.Success();
        }

        Profile = profile;
        MarkChanged(OrganizationSettingsSection.Profile);
        return Result.Success();
    }

    public Result UpdateContact(OrganizationContactInformation contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        if (Contact == contact)
        {
            return Result.Success();
        }

        Contact = contact;
        MarkChanged(OrganizationSettingsSection.Contact);
        return Result.Success();
    }

    public Result UpdateAddress(OrganizationAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (Address == address)
        {
            return Result.Success();
        }

        Address = address;
        MarkChanged(OrganizationSettingsSection.Address);
        return Result.Success();
    }

    public Result UpdateRegionalSettings(OrganizationRegionalSettings regional)
    {
        ArgumentNullException.ThrowIfNull(regional);
        if (Regional == regional)
        {
            return Result.Success();
        }

        Regional = regional;
        MarkChanged(OrganizationSettingsSection.Regional);
        return Result.Success();
    }

    public Result UpdateOperationalSettings(OrganizationOperationalSettings operational)
    {
        ArgumentNullException.ThrowIfNull(operational);
        if (Operational == operational)
        {
            return Result.Success();
        }

        Operational = operational;
        MarkChanged(OrganizationSettingsSection.Operational);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    private void MarkChanged(OrganizationSettingsSection section)
    {
        Version++;
        RaiseDomainEvent(
            new OrganizationSettingsChangedDomainEvent(
                Id,
                OrganizationId,
                section,
                Version));
    }
}
