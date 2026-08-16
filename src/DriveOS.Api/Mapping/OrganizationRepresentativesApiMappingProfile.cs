using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Activate;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Create;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.End;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Reactivate;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.SetPrimaryOwner;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Suspend;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.UpdateAuthority;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationRepresentativesApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<
            OrganizationRepresentativeResponse,
            OrganizationRepresentativeResponseContract
        >();
        configuration.CreateMap<
            OrganizationRepresentativeListItem,
            OrganizationRepresentativeListItemContract
        >();
        configuration.CreateMap<
            CreateOrganizationRepresentativeApiModel,
            CreateOrganizationRepresentativeCommand
        >();
        configuration.CreateMap<
            UpdateOrganizationRepresentativeAuthorityApiModel,
            UpdateOrganizationRepresentativeAuthorityCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationRepresentativeStatusApiModel,
            ActivateOrganizationRepresentativeCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationRepresentativeStatusApiModel,
            SetPrimaryOrganizationOwnerCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationRepresentativeStatusWithReasonApiModel,
            SuspendOrganizationRepresentativeCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationRepresentativeStatusWithReasonApiModel,
            ReactivateOrganizationRepresentativeCommand
        >();
        configuration.CreateMap<
            EndOrganizationRepresentativeApiModel,
            EndOrganizationRepresentativeCommand
        >();
    }
}
