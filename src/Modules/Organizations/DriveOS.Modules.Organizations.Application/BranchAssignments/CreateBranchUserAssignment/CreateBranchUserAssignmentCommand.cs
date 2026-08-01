using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.CreateBranchUserAssignment;

public sealed record CreateBranchUserAssignmentCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    UserId UserId,
    BranchAssignmentRole Role,
    BranchAssignmentType AssignmentType,
    DateTimeOffset? PlannedEndAtUtc)
    : ICommand<BranchUserAssignmentId>;