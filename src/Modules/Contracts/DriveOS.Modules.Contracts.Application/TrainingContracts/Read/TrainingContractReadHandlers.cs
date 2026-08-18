using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Read;

public sealed class GetTrainingContractQueryHandler(ITrainingContractReadService readService)
    : IQueryHandler<GetTrainingContractQuery, TrainingContractDetailResponse>
{
    public async Task<Result<TrainingContractDetailResponse>> Handle(GetTrainingContractQuery query, CancellationToken cancellationToken)
    {
        TrainingContractDetailResponse? contract = await readService.GetAsync(query.OrganizationId, query.ContractId, cancellationToken);
        return contract is null
            ? Result.Failure<TrainingContractDetailResponse>(TrainingContractReadErrors.NotFound)
            : Result.Success(contract);
    }
}

public sealed class GetTrainingContractsQueryHandler(ITrainingContractReadService readService)
    : IQueryHandler<GetTrainingContractsQuery, IReadOnlyList<TrainingContractListItemResponse>>
{
    public async Task<Result<IReadOnlyList<TrainingContractListItemResponse>>> Handle(GetTrainingContractsQuery query, CancellationToken cancellationToken) =>
        Result.Success(await readService.ListAsync(query.OrganizationId, query.StudentId, cancellationToken));
}

public static class TrainingContractReadErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Contracts.TrainingContract.NotFound",
        "errors.contracts.trainingContract.notFound");
}
