using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Read;

public sealed class SearchTrainingContractsQueryHandler(ITrainingContractReadService readService)
    : IQueryHandler<SearchTrainingContractsQuery, PagedResult<TrainingContractListItemResponse>>
{
    public async Task<Result<PagedResult<TrainingContractListItemResponse>>> Handle(
        SearchTrainingContractsQuery query,
        CancellationToken cancellationToken) =>
        Result.Success(await readService.SearchAsync(query, cancellationToken));
}
