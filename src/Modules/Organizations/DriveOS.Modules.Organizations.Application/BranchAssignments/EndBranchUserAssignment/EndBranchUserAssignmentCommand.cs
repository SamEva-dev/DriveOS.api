using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchAssignments.EndBranchUserAssignment;

public sealed record EndBranchUserAssignmentCommand(
    OrganizationId OrganizationId,
    BranchUserAssignmentId AssignmentId,
    string Reason
) : ICommand;
