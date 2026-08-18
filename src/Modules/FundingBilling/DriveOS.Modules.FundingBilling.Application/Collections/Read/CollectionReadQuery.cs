using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.Collections.Read;
public sealed record GetOverdueItemsQuery(OrganizationId OrganizationId, DateOnly BusinessDate) : IQuery<IReadOnlyCollection<OverdueItemResponse>>;
internal sealed class GetOverdueItemsQueryHandler(ICollectionReadService service) : IQueryHandler<GetOverdueItemsQuery, IReadOnlyCollection<OverdueItemResponse>>
{
    public async Task<Result<IReadOnlyCollection<OverdueItemResponse>>> Handle(GetOverdueItemsQuery query, CancellationToken cancellationToken) => Result.Success(await service.ListOverdueAsync(query.OrganizationId, query.BusinessDate, cancellationToken));
}
