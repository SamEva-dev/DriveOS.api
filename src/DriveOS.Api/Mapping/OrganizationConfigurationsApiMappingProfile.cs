using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Archive;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.CreateDraft;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Publish;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.UpdateDraft;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationConfigurationsApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<
            OrganizationConfigurationResponse,
            OrganizationConfigurationResponseContract
        >();

        configuration.CreateMap<
            OrganizationConfigurationListItemResponse,
            OrganizationConfigurationListItemResponseContract
        >();

        configuration.CreateMap<
            CreateOrganizationConfigurationDraftApiModel,
            CreateOrganizationConfigurationDraftCommand
        >();

        configuration.CreateMap<
            UpdateOrganizationConfigurationDraftApiModel,
            UpdateOrganizationConfigurationDraftCommand
        >();

        configuration.CreateMap<
            PublishOrganizationConfigurationApiModel,
            PublishOrganizationConfigurationCommand
        >();

        configuration.CreateMap<
            ArchiveOrganizationConfigurationApiModel,
            ArchiveOrganizationConfigurationCommand
        >();
    }
}
