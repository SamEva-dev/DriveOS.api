namespace DriveOS.Modules.Organizations.Application.Branches.Models;

public sealed record BranchResponse(
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
    DateTimeOffset? LastModifiedAtUtc
);
