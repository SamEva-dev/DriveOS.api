namespace DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;

public sealed record OrganizationListItem(
    Guid Id,
    string LegalName,
    string CountryCode,
    string Type,
    string Status,
    DateTimeOffset CreatedAtUtc
);
