using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.StatusHistory;

public sealed record GetBranchStatusHistoryQuery(OrganizationId OrganizationId, BranchId BranchId)
    : IQuery<IReadOnlyList<BranchStatusHistoryItem>>;
