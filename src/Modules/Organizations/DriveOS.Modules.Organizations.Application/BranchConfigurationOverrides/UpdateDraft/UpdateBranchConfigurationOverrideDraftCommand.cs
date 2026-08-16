using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.UpdateDraft;

public sealed record UpdateBranchConfigurationOverrideDraftCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    BranchConfigurationOverrideId OverrideId,
    string PayloadJson,
    int ExpectedRevision
) : ICommand;
