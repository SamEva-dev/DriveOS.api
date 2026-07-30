using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Managers.AssignBranchManager;

public sealed record AssignBranchManagerCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    UserId ManagerUserId,
    DateTimeOffset? EffectiveFromUtc)
    : ICommand;