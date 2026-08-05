using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Activate;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Archive;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Create;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Update;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationLegalProfilesApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<OrganizationLegalProfileResponse, OrganizationLegalProfileResponseContract>();
        configuration.CreateMap<CreateOrganizationLegalProfileApiModel, CreateOrganizationLegalProfileCommand>();
        configuration.CreateMap<UpdateOrganizationLegalProfileApiModel, UpdateOrganizationLegalProfileCommand>();
        configuration.CreateMap<ChangeOrganizationLegalProfileStatusApiModel, ActivateOrganizationLegalProfileCommand>();
        configuration.CreateMap<ChangeOrganizationLegalProfileStatusApiModel, ArchiveOrganizationLegalProfileCommand>();
    }
}
