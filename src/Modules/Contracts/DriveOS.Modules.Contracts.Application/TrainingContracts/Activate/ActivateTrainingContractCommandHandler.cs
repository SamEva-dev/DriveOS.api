using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Activate;

public sealed class ActivateTrainingContractCommandHandler(
    ITrainingContractRepository contracts,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<ActivateTrainingContractCommand, ActivateTrainingContractResponse>
{
    public async Task<Result<ActivateTrainingContractResponse>> Handle(
        ActivateTrainingContractCommand command,
        CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<ActivateTrainingContractResponse>(TrainingContractErrors.NotFound);

        DateTimeOffset now = clock.UtcNow;
        Result activation = contract.Activate(command.ActorUserId, now);
        if (activation.IsFailure)
            return Result.Failure<ActivateTrainingContractResponse>(activation.Error);

        contract.SetModifiedAudit(now, command.ActorUserId);
        await uow.CommitAsync(cancellationToken);

        return Result.Success(new ActivateTrainingContractResponse(
            contract.Id.Value,
            contract.Status.ToString(),
            contract.ActivatedAtUtc!.Value));
    }
}
