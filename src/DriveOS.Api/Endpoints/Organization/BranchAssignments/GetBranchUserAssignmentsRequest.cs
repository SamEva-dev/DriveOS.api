namespace DriveOS.Api.Endpoints.Organization.BranchAssignments;

public sealed record GetBranchUserAssignmentsRequest(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null,
    string? Role = null,
    string? AssignmentType = null,
    string? SortBy = null,
    string? SortDirection = null
);
