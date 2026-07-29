namespace DriveOS.Api.Endpoints.Branches;

public sealed record BranchListItemResponse(
    Guid Id,
    string Name,
    string Code,
    string BranchType,
    string Status,
    bool IsPrimary,
    string City,
    string CountryCode,
    string TimeZoneId);
