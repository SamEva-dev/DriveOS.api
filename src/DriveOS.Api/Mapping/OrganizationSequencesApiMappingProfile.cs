using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Organization.OrganizationSequences;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Archive;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Create;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Reactivate;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Reserve;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Suspend;

namespace DriveOS.Api.Mapping;

public sealed class OrganizationSequencesApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<
            OrganizationSequenceResponse,
            OrganizationSequenceResponseContract
        >();
        configuration.CreateMap<
            OrganizationSequenceListItem,
            OrganizationSequenceListItemContract
        >();
        configuration.CreateMap<
            CreateOrganizationSequenceApiModel,
            CreateOrganizationSequenceCommand
        >();
        configuration.CreateMap<
            ReserveOrganizationSequenceNumberApiModel,
            ReserveOrganizationSequenceNumberCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationSequenceStatusApiModel,
            SuspendOrganizationSequenceCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationSequenceStatusApiModel,
            ReactivateOrganizationSequenceCommand
        >();
        configuration.CreateMap<
            ChangeOrganizationSequenceStatusApiModel,
            ArchiveOrganizationSequenceCommand
        >();
    }
}
