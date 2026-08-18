using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Terminate;

public sealed class TerminateTrainingContractCommandHandler(
    ITrainingContractRepository contracts,
    IContractsUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<TerminateTrainingContractCommand, TerminateTrainingContractResponse>
{
    public async Task<Result<TerminateTrainingContractResponse>> Handle(
        TerminateTrainingContractCommand command,
        CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<TerminateTrainingContractResponse>(TrainingContractErrors.NotFound);

        DateTimeOffset now = clock.UtcNow;
        Result result = contract.Terminate(command.Reason, command.EffectiveDate, command.ActorUserId, now);
        if (result.IsFailure)
            return Result.Failure<TerminateTrainingContractResponse>(result.Error);

        contract.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new TerminateTrainingContractResponse(
            contract.Id.Value,
            contract.Status.ToString(),
            contract.TerminationEffectiveDate!.Value,
            contract.TerminatedAtUtc!.Value));
    }
}
