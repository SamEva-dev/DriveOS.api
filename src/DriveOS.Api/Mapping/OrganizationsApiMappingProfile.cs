using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.Organizations;
using DriveOS.Modules.Organizations.Application.Organizations.CreateOrganization;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationsApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<OrganizationResponse, GetOrganizationResponse>();

        configuration.CreateMap<OrganizationListItem, OrganizationListItemResponse>();

        configuration.CreateMap<CreateOrganizationRequest, CreateOrganizationCommand>();
    }
}
