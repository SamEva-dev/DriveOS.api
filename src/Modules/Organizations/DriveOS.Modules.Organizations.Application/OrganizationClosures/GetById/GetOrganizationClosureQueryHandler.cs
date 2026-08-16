using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.GetById;

internal sealed class GetOrganizationClosureQueryHandler(IOrganizationClosureRepository repository)
    : IQueryHandler<GetOrganizationClosureQuery, OrganizationClosureModel>
{
    public async Task<Result<OrganizationClosureModel>> Handle(
        GetOrganizationClosureQuery query,
        CancellationToken cancellationToken
    )
    {
        OrganizationClosure? closure = await repository.GetByIdAsync(
            query.ClosureId,
            cancellationToken
        );
        return closure is null
            ? Result.Failure<OrganizationClosureModel>(OrganizationClosureErrors.NotFound)
            : Result.Success(OrganizationClosureModel.FromDomain(closure));
    }
}
