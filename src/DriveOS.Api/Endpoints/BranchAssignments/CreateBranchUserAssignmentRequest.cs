using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;

namespace DriveOS.Api.Endpoints
    .BranchAssignments;

public sealed record
    CreateBranchUserAssignmentRequest(
        Guid UserId,
        BranchAssignmentRole Role,
        BranchAssignmentType AssignmentType,
        DateTimeOffset? PlannedEndAtUtc);