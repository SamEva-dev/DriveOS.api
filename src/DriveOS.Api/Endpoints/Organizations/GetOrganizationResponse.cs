namespace DriveOS.Api.Endpoints.Organizations;

public sealed record GetOrganizationResponse(
    Guid Id,
    string LegalName,
    string CountryCode,
    string Type,
    string Status,
    DateTimeOffset CreatedAtUtc,
    Guid? CreatedByUserId,
    DateTimeOffset? LastModifiedAtUtc,
    Guid? LastModifiedByUserId);