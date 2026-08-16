namespace DriveOS.Api.Endpoints.Organization.BranchAssignments;

public sealed record GetUserBranchAssignmentsRequest(
    int PageNumber = 1,
    int PageSize = 20,
    string? Status = null,
    string? Role = null,
    string? AssignmentType = null,
    string? SortBy = null,
    string? SortDirection = null
);
