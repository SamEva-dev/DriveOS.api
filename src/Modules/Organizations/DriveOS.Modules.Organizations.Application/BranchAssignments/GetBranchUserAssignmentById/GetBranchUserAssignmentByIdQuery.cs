using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.Models;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .BranchAssignments.GetBranchUserAssignmentById;

public sealed record GetBranchUserAssignmentByIdQuery(
    OrganizationId OrganizationId,
    BranchUserAssignmentId AssignmentId)
    : IQuery<BranchUserAssignmentItem>;