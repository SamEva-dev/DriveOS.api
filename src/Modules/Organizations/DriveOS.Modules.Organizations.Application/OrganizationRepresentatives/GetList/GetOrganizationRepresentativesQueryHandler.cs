using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.GetList;

internal sealed class GetOrganizationRepresentativesQueryHandler(
    IOrganizationRepresentativeReadService readService
)
    : IQueryHandler<
        GetOrganizationRepresentativesQuery,
        IReadOnlyCollection<OrganizationRepresentativeListItem>
    >
{
    public async Task<Result<IReadOnlyCollection<OrganizationRepresentativeListItem>>> Handle(
        GetOrganizationRepresentativesQuery q,
        CancellationToken ct
    ) => Result.Success(await readService.GetListAsync(q.OrganizationId, q.Status, ct));
}
