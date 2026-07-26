using DriveOS.Modules.Organizations.Domain.Organizations.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Organizations;

public sealed class Organization :
    AggregateRoot<OrganizationId>,
    IAuditableEntity
{
    private Organization()
    {
    }

    private Organization(
        OrganizationId id,
        string legalName,
        string countryCode,
        OrganizationType type)
        : base(id)
    {
        LegalName = legalName;
        CountryCode = countryCode;
        Type = type;
        Status = OrganizationStatus.Draft;
    }

    public string LegalName { get; private set; } = string.Empty;

    public string CountryCode { get; private set; } = string.Empty;

    public OrganizationType Type { get; private set; }

    public OrganizationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public UserId? CreatedByUserId { get; private set; }

    public DateTimeOffset? LastModifiedAtUtc { get; private set; }

    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<Organization> Create(
    OrganizationId id,
    string legalName,
    string countryCode,
    OrganizationType type)
    {
        if (id.IsEmpty)
        {
            return Result.Failure<Organization>(
                OrganizationErrors.EmptyId);
        }

        string normalizedLegalName =
            legalName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedLegalName))
        {
            return Result.Failure<Organization>(
                OrganizationErrors.EmptyLegalName);
        }

        const int maximumLegalNameLength = 200;

        if (normalizedLegalName.Length > maximumLegalNameLength)
        {
            return Result.Failure<Organization>(
                OrganizationErrors.LegalNameTooLong(
                    maximumLegalNameLength));
        }

        string normalizedCountryCode =
            countryCode?.Trim().ToUpperInvariant()
            ?? string.Empty;

        if (!IsValidCountryCode(normalizedCountryCode))
        {
            return Result.Failure<Organization>(
                OrganizationErrors.InvalidCountryCode);
        }

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<Organization>(
                OrganizationErrors.InvalidOrganizationType);
        }

        var organization = new Organization(
            id,
            normalizedLegalName,
            normalizedCountryCode,
            type);

        organization.RaiseDomainEvent(
            new OrganizationCreatedDomainEvent(
                organization.Id,
                organization.LegalName,
                organization.CountryCode,
                organization.Type));

        return Result.Success(organization);
    }

    public void SetCreatedAudit(
    DateTimeOffset createdAtUtc,
    UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
        {
            return;
        }

        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(
        DateTimeOffset modifiedAtUtc,
        UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    private static bool IsValidCountryCode(string countryCode)
    {
        return countryCode.Length == 2
            && countryCode.All(char.IsLetter);
    }
}