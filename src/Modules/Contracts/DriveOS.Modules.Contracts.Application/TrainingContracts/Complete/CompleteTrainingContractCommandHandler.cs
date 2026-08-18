using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Complete;

public sealed class CompleteTrainingContractCommandHandler(ITrainingContractRepository contracts, IContractsUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<CompleteTrainingContractCommand, CompleteTrainingContractResponse>
{
    public async Task<Result<CompleteTrainingContractResponse>> Handle(CompleteTrainingContractCommand command, CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<CompleteTrainingContractResponse>(TrainingContractErrors.NotFound);

        DateTimeOffset now = clock.UtcNow;
        Result result = contract.Complete(command.Note, command.EffectiveDate, command.ActorUserId, now);
        if (result.IsFailure) return Result.Failure<CompleteTrainingContractResponse>(result.Error);

        contract.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(new CompleteTrainingContractResponse(contract.Id.Value, contract.Status.ToString(), contract.CompletionEffectiveDate!.Value, contract.CompletedAtUtc!.Value));
    }
}
