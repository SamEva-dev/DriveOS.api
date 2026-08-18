using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Expire;

public sealed class ExpireTrainingContractCommandHandler(ITrainingContractRepository contracts, IContractsUnitOfWork unitOfWork, IClock clock)
    : ICommandHandler<ExpireTrainingContractCommand, ExpireTrainingContractResponse>
{
    public async Task<Result<ExpireTrainingContractResponse>> Handle(ExpireTrainingContractCommand command, CancellationToken cancellationToken)
    {
        TrainingContract? contract = await contracts.GetByIdAsync(command.ContractId, cancellationToken);
        if (contract is null || contract.OrganizationId != command.OrganizationId)
            return Result.Failure<ExpireTrainingContractResponse>(TrainingContractErrors.NotFound);

        DateTimeOffset now = clock.UtcNow;
        Result result = contract.Expire(command.ActorUserId, now);
        if (result.IsFailure) return Result.Failure<ExpireTrainingContractResponse>(result.Error);

        contract.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(new ExpireTrainingContractResponse(contract.Id.Value, contract.Status.ToString(), contract.ExpirationEffectiveDate!.Value, contract.ExpiredAtUtc!.Value));
    }
}
