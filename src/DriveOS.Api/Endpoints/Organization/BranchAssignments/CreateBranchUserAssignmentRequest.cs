using DriveOS.Modules.Organizations.Domain.BranchAssignments;

namespace DriveOS.Api.Endpoints.Organization.BranchAssignments;

public sealed record CreateBranchUserAssignmentRequest(
    Guid UserId,
    BranchAssignmentRole Role,
    BranchAssignmentType AssignmentType,
    DateTimeOffset? PlannedEndAtUtc
);
