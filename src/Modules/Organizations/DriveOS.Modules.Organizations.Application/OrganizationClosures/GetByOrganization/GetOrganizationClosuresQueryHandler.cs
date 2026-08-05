using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.GetByOrganization;

internal sealed class GetOrganizationClosuresQueryHandler(IOrganizationClosureRepository repository)
    : IQueryHandler<GetOrganizationClosuresQuery, IReadOnlyList<OrganizationClosureModel>>
{
    public async Task<Result<IReadOnlyList<OrganizationClosureModel>>> Handle(GetOrganizationClosuresQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<OrganizationClosure> closures = await repository.ListByOrganizationAsync(query.OrganizationId, cancellationToken);
        return Result.Success<IReadOnlyList<OrganizationClosureModel>>(closures.Select(OrganizationClosureModel.FromDomain).ToArray());
    }
}
