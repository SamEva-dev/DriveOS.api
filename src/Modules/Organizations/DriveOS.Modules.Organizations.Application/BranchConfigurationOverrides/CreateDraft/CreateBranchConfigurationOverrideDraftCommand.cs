using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.CreateDraft;

public sealed record CreateBranchConfigurationOverrideDraftCommand(
    OrganizationId OrganizationId,
    BranchId BranchId,
    OrganizationConfigurationId BaseConfigurationId,
    int VersionNumber,
    string CountryCode,
    string PayloadJson)
    : ICommand<BranchConfigurationOverrideId>;
