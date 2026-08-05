using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSequences.Models;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSequences.GetList;

internal sealed class GetOrganizationSequencesQueryHandler(
    IOrganizationSequenceReadService readService)
    : IQueryHandler<GetOrganizationSequencesQuery, IReadOnlyList<OrganizationSequenceListItem>>
{
    public async Task<Result<IReadOnlyList<OrganizationSequenceListItem>>> Handle(
        GetOrganizationSequencesQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OrganizationSequenceListItem> items =
            await readService.GetListAsync(
                query.OrganizationId,
                query.BranchId,
                cancellationToken);

        return Result.Success(items);
    }
}
