using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.Managers.GetBranchManagerHistory;

public sealed record GetBranchManagerHistoryQuery(OrganizationId OrganizationId, BranchId BranchId)
    : IQuery<IReadOnlyList<BranchManagerAssignmentItem>>;
