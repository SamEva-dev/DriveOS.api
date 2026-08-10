namespace DriveOS.Api.Endpoints.Organization.Branches;

public sealed record GetBranchResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Code,
    string BranchType,
    string Status,
    bool IsPrimary,
    string AddressLine1,
    string? AddressLine2,
    string PostalCode,
    string City,
    string CountryCode,
    string TimeZoneId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);
