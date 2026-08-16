namespace DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;

public sealed record OrganizationResponse(
    Guid Id,
    string LegalName,
    string CountryCode,
    string Type,
    string Status,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId
);
