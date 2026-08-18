using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Contracts.Application.Auditing;
using DriveOS.Modules.Contracts.Application.ContractDocuments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Read;

public sealed class GetTrainingContractHistoryQueryHandler(
    ITrainingContractReadService contractReadService,
    IContractDocumentReadService documentReadService,
    IContractAuditReadService auditReadService)
    : IQueryHandler<GetTrainingContractHistoryQuery, TrainingContractHistoryResponse>
{
    public async Task<Result<TrainingContractHistoryResponse>> Handle(
        GetTrainingContractHistoryQuery query,
        CancellationToken cancellationToken)
    {
        TrainingContractDetailResponse? contract = await contractReadService.GetAsync(
            query.OrganizationId,
            query.ContractId,
            cancellationToken);

        if (contract is null)
            return Result.Failure<TrainingContractHistoryResponse>(TrainingContractReadErrors.NotFound);

        IReadOnlyList<ContractDocumentResponse> documents = await documentReadService.ListAsync(
            query.OrganizationId,
            query.ContractId,
            cancellationToken);

        IReadOnlyList<ContractAuditEntryResponse> audit = await auditReadService.ListAsync(
            query.OrganizationId,
            query.ContractId,
            cancellationToken);

        return Result.Success(new TrainingContractHistoryResponse(
            contract.Id,
            contract.ContractNumber,
            contract.Status,
            contract.CurrentVersionNumber,
            contract.Versions,
            contract.Amendments,
            documents,
            contract.CurrentSignatureProcess,
            audit));
    }
}
