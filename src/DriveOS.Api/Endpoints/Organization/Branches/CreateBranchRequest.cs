using DriveOS.Modules.Organizations.Domain.Branches;

namespace DriveOS.Api.Endpoints.Organization.Branches;

public sealed record CreateBranchRequest(
    string Name,
    string Code,
    BranchType BranchType,
    string AddressLine1,
    string? AddressLine2,
    string PostalCode,
    string City,
    string TimeZoneId,
    bool IsPrimary
);
