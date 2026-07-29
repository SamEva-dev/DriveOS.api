using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.Branches.SetPrimaryBranch;

public sealed record SetPrimaryBranchCommand(
    OrganizationId OrganizationId,
    BranchId BranchId)
    : ICommand;
