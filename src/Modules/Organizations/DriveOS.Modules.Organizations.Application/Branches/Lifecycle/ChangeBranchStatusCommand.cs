using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Lifecycle;

public sealed record ChangeBranchStatusCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    BranchStatus TargetStatus,
    string Reason)
    : ICommand;