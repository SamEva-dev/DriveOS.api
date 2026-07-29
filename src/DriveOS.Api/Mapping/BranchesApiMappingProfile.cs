using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.Abstractions.Profiles;
using DriveOS.Api.Endpoints.Branches;
using DriveOS.Modules.Organizations.Application.Branches.Models;

namespace DriveOS.Api.Mapping;

public sealed class BranchesApiMappingProfile : MappingProfile
{
    public override void Configure(IMappingConfiguration configuration)
    {
        configuration.CreateMap<BranchResponse, GetBranchResponse>();
        configuration.CreateMap<BranchListItem, BranchListItemResponse>();
    }
}
