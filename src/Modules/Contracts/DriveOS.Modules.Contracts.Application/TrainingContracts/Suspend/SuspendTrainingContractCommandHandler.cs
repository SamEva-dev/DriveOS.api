using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Suspend;

public sealed class SuspendTrainingContractCommandHandler(
    ITrainingContractRepository contracts,
    IContractsUnitOfWork uow,
    IClock clock) : ICommandHandler<SuspendTrainingContractCommand, SuspendTrainingContractResponse>
{
    public async Task<Result<SuspendTrainingContractResponse>> Handle(
        SuspendTrainingContractCommand command, CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<SuspendTrainingContractResponse>(TrainingContractErrors.NotFound);

        DateTimeOffset now = clock.UtcNow;
        Result result = contract.Suspend(command.Reason, command.EffectiveDate, command.ExpectedResumeDate, command.ActorUserId, now);
        if (result.IsFailure)
            return Result.Failure<SuspendTrainingContractResponse>(result.Error);

        contract.SetModifiedAudit(now, command.ActorUserId);
        await uow.CommitAsync(cancellationToken);

        return Result.Success(new SuspendTrainingContractResponse(
            contract.Id.Value, contract.Status.ToString(), contract.SuspensionEffectiveDate!.Value,
            contract.SuspensionExpectedResumeDate, contract.SuspendedAtUtc!.Value));
    }
}
