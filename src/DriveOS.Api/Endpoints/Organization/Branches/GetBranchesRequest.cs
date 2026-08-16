namespace DriveOS.Api.Endpoints.Organization.Branches;

public sealed record GetBranchesRequest(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = null
);
