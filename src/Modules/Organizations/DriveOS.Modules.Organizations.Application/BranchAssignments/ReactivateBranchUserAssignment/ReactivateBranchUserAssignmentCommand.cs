using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.ReactivateBranchUserAssignment;

public sealed record ReactivateBranchUserAssignmentCommand(
    OrganizationId OrganizationId,
    BranchUserAssignmentId AssignmentId,
    string Reason)
    : ICommand;