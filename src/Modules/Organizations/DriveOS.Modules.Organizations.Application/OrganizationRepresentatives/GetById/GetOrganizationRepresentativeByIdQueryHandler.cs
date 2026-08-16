using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.GetById;

internal sealed class GetOrganizationRepresentativeByIdQueryHandler(
    IOrganizationRepresentativeReadService readService
) : IQueryHandler<GetOrganizationRepresentativeByIdQuery, OrganizationRepresentativeResponse>
{
    public async Task<Result<OrganizationRepresentativeResponse>> Handle(
        GetOrganizationRepresentativeByIdQuery q,
        CancellationToken ct
    )
    {
        var item = await readService.GetByIdAsync(q.OrganizationId, q.RepresentativeId, ct);
        return item is null
            ? Result.Failure<OrganizationRepresentativeResponse>(
                OrganizationRepresentativeErrors.NotFound
            )
            : Result.Success(item);
    }
}
