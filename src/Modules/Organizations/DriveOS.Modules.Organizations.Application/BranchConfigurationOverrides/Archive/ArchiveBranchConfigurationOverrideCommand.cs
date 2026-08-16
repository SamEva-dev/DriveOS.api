using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Archive;

public sealed record ArchiveBranchConfigurationOverrideCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    BranchConfigurationOverrideId OverrideId,
    int ExpectedRevision
) : ICommand;
