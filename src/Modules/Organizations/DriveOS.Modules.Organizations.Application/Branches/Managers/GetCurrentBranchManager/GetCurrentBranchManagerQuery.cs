using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.Managers.GetCurrentBranchManager;

public sealed record GetCurrentBranchManagerQuery(OrganizationId OrganizationId, BranchId BranchId)
    : IQuery<BranchManagerAssignmentItem>;
