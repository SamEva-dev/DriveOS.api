namespace DriveOS.Api.Endpoints.Organization.Organizations;

public sealed record OrganizationListItemResponse(
    Guid Id,
    string LegalName,
    string CountryCode,
    string Type,
    string Status,
    DateTimeOffset CreatedAtUtc);