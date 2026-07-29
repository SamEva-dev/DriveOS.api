namespace DriveOS.Modules.Organizations.Application.Branches.Models;

public sealed record BranchListItem(
    Guid Id,
    string Name,
    string Code,
    string BranchType,
    string Status,
    bool IsPrimary,
    string City,
    string CountryCode,
    string TimeZoneId);
