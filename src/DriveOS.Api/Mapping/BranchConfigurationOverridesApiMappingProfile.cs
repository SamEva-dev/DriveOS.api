using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Archive;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.CreateDraft;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Publish;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.UpdateDraft;

namespace DriveOS.Api.Mapping;

public sealed class BranchConfigurationOverridesApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<BranchConfigurationOverrideResponse,
            BranchConfigurationOverrideResponseContract>();

        configuration.CreateMap<BranchConfigurationOverrideListItemResponse,
            BranchConfigurationOverrideListItemResponseContract>();

        configuration.CreateMap<CreateBranchConfigurationOverrideDraftApiModel,
            CreateBranchConfigurationOverrideDraftCommand>();

        configuration.CreateMap<UpdateBranchConfigurationOverrideDraftApiModel,
            UpdateBranchConfigurationOverrideDraftCommand>();

        configuration.CreateMap<PublishBranchConfigurationOverrideApiModel,
            PublishBranchConfigurationOverrideCommand>();

        configuration.CreateMap<ArchiveBranchConfigurationOverrideApiModel,
            ArchiveBranchConfigurationOverrideCommand>();
    }
}
