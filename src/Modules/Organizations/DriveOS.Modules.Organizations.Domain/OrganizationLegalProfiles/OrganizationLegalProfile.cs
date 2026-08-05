using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

public sealed class OrganizationLegalProfile :
    AggregateRoot<OrganizationLegalProfileId>,
    IAuditableEntity
{
    private OrganizationLegalProfile() { }

    private OrganizationLegalProfile(
        OrganizationLegalProfileId id,
        OrganizationId organizationId,
        OrganizationLegalForm legalForm,
        string registrationNumber,
        string? taxNumber,
        string? tradeName,
        DateOnly? incorporationDate,
        RegisteredAddress registeredAddress)
        : base(id)
    {
        OrganizationId = organizationId;
        LegalForm = legalForm;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        TradeName = tradeName;
        IncorporationDate = incorporationDate;
        RegisteredAddress = registeredAddress;
        Status = OrganizationLegalProfileStatus.Draft;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public OrganizationLegalForm LegalForm { get; private set; }
    public string RegistrationNumber { get; private set; } = string.Empty;
    public string? TaxNumber { get; private set; }
    public string? TradeName { get; private set; }
    public DateOnly? IncorporationDate { get; private set; }
    public RegisteredAddress RegisteredAddress { get; private set; } = null!;
    public OrganizationLegalProfileStatus Status { get; private set; }
    public int Revision { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<OrganizationLegalProfile> Create(
        OrganizationLegalProfileId id,
        OrganizationId organizationId,
        OrganizationLegalForm legalForm,
        string registrationNumber,
        string? taxNumber,
        string? tradeName,
        DateOnly? incorporationDate,
        RegisteredAddress registeredAddress,
        string organizationCountryCode)
    {
        if (id.IsEmpty)
            return Result.Failure<OrganizationLegalProfile>(OrganizationLegalProfileErrors.EmptyId);
        if (organizationId.IsEmpty)
            return Result.Failure<OrganizationLegalProfile>(OrganizationLegalProfileErrors.EmptyOrganizationId);
        if (!Enum.IsDefined(legalForm))
            return Result.Failure<OrganizationLegalProfile>(OrganizationLegalProfileErrors.InvalidLegalForm);

        string normalizedRegistrationNumber = NormalizeRequired(registrationNumber);
        if (normalizedRegistrationNumber.Length is < 2 or > 80)
            return Result.Failure<OrganizationLegalProfile>(OrganizationLegalProfileErrors.RegistrationNumberRequired);

        if (!string.Equals(
                registeredAddress.CountryCode,
                organizationCountryCode?.Trim().ToUpperInvariant(),
                StringComparison.Ordinal))
        {
            return Result.Failure<OrganizationLegalProfile>(OrganizationLegalProfileErrors.CountryMismatch);
        }

        var profile = new OrganizationLegalProfile(
            id,
            organizationId,
            legalForm,
            normalizedRegistrationNumber,
            NormalizeOptional(taxNumber, 80),
            NormalizeOptional(tradeName, 200),
            incorporationDate,
            registeredAddress);

        profile.RaiseDomainEvent(new OrganizationLegalProfileCreatedDomainEvent(
            profile.Id,
            profile.OrganizationId,
            profile.RegistrationNumber));

        return Result.Success(profile);
    }

    public Result Update(
        OrganizationLegalForm legalForm,
        string registrationNumber,
        string? taxNumber,
        string? tradeName,
        DateOnly? incorporationDate,
        RegisteredAddress registeredAddress,
        string organizationCountryCode)
    {
        if (Status == OrganizationLegalProfileStatus.Archived)
            return Result.Failure(OrganizationLegalProfileErrors.ArchivedProfileCannotBeChanged);
        if (!Enum.IsDefined(legalForm))
            return Result.Failure(OrganizationLegalProfileErrors.InvalidLegalForm);

        string normalizedRegistrationNumber = NormalizeRequired(registrationNumber);
        if (normalizedRegistrationNumber.Length is < 2 or > 80)
            return Result.Failure(OrganizationLegalProfileErrors.RegistrationNumberRequired);
        if (!string.Equals(registeredAddress.CountryCode, organizationCountryCode.Trim().ToUpperInvariant(), StringComparison.Ordinal))
            return Result.Failure(OrganizationLegalProfileErrors.CountryMismatch);

        LegalForm = legalForm;
        RegistrationNumber = normalizedRegistrationNumber;
        TaxNumber = NormalizeOptional(taxNumber, 80);
        TradeName = NormalizeOptional(tradeName, 200);
        IncorporationDate = incorporationDate;
        RegisteredAddress = registeredAddress;
        Revision++;
        return Result.Success();
    }

    public Result Activate()
    {
        if (Status == OrganizationLegalProfileStatus.Archived)
            return Result.Failure(OrganizationLegalProfileErrors.ArchivedProfileCannotBeChanged);

        if (Status == OrganizationLegalProfileStatus.Active)
            return Result.Success();

        Status = OrganizationLegalProfileStatus.Active;
        Revision++;
        RaiseDomainEvent(new OrganizationLegalProfileActivatedDomainEvent(Id, OrganizationId, Revision));
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == OrganizationLegalProfileStatus.Archived)
            return Result.Success();

        Status = OrganizationLegalProfileStatus.Archived;
        Revision++;
        RaiseDomainEvent(new OrganizationLegalProfileArchivedDomainEvent(Id, OrganizationId, Revision));
        return Result.Success();
    }

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

    private static string NormalizeRequired(string? value) => value?.Trim().ToUpperInvariant() ?? string.Empty;

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
