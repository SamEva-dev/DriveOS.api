using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.OrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.CreateOrganizationSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateAddress;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateContact;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateOperationalSettings;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateProfile;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.UpdateRegionalSettings;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationSettingsApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<
            OrganizationSettingsResponse,
            OrganizationSettingsResponseContract
        >();

        configuration.CreateMap<
            CreateOrganizationSettingsApiModel,
            CreateOrganizationSettingsCommand
        >();

        configuration.CreateMap<
            UpdateOrganizationProfileApiModel,
            UpdateOrganizationProfileCommand
        >();

        configuration.CreateMap<
            UpdateOrganizationContactApiModel,
            UpdateOrganizationContactCommand
        >();

        configuration.CreateMap<
            UpdateOrganizationAddressApiModel,
            UpdateOrganizationAddressCommand
        >();

        configuration.CreateMap<
            UpdateOrganizationRegionalSettingsApiModel,
            UpdateOrganizationRegionalSettingsCommand
        >();

        configuration.CreateMap<
            UpdateOrganizationOperationalSettingsApiModel,
            UpdateOrganizationOperationalSettingsCommand
        >();
    }
}
