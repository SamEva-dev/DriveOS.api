using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Contracts.Application.Persistence;
using DriveOS.Modules.Contracts.Domain.TrainingContracts;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Contracts.Application.TrainingContracts.Signatories;

internal static class SignatoryLoader
{
    public static async Task<Result<TrainingContract>> LoadAsync(ITrainingContractRepository repository, DriveOS.SharedKernel.Identifiers.OrganizationId organizationId, DriveOS.SharedKernel.Identifiers.TrainingContractId contractId, CancellationToken ct)
    {
        TrainingContract? contract = await repository.GetByIdAsync(contractId, ct);
        return contract is null || contract.OrganizationId != organizationId
            ? Result.Failure<TrainingContract>(TrainingContractErrors.NotFound)
            : Result.Success(contract);
    }
}

public sealed class AddTrainingContractSignatoryCommandHandler(ITrainingContractRepository repository, IContractsUnitOfWork uow, IClock clock) : ICommandHandler<AddTrainingContractSignatoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(AddTrainingContractSignatoryCommand command, CancellationToken ct)
    {
        Result<TrainingContract> loaded = await SignatoryLoader.LoadAsync(repository, command.OrganizationId, command.ContractId, ct);
        if (loaded.IsFailure) return Result.Failure<Guid>(loaded.Error);
        if (!Enum.TryParse<TrainingContractSignatoryKind>(command.Kind, true, out var kind)) return Result.Failure<Guid>(TrainingContractErrors.InvalidSignatory);
        Result<TrainingContractSignatory> added = loaded.Value.AddSignatory(kind, command.PersonId, command.RepresentedOrganizationId, command.DisplayName, command.SigningOrder, command.IsRequired, command.AuthorityReference);
        if (added.IsFailure) return Result.Failure<Guid>(added.Error);
        loaded.Value.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await uow.CommitAsync(ct);
        return Result.Success(added.Value.Id.Value);
    }
}

public sealed class UpdateTrainingContractSignatoryCommandHandler(ITrainingContractRepository repository, IContractsUnitOfWork uow, IClock clock) : ICommandHandler<UpdateTrainingContractSignatoryCommand>
{
    public async Task<Result> Handle(UpdateTrainingContractSignatoryCommand command, CancellationToken ct)
    {
        Result<TrainingContract> loaded = await SignatoryLoader.LoadAsync(repository, command.OrganizationId, command.ContractId, ct);
        if (loaded.IsFailure) return Result.Failure(loaded.Error);
        Result result = loaded.Value.UpdateSignatory(command.SignatoryId, command.SigningOrder, command.IsRequired, command.DisplayName, command.AuthorityReference);
        if (result.IsFailure) return result;
        loaded.Value.SetModifiedAudit(clock.UtcNow, command.ActorUserId); await uow.CommitAsync(ct); return Result.Success();
    }
}

public sealed class RemoveTrainingContractSignatoryCommandHandler(ITrainingContractRepository repository, IContractsUnitOfWork uow, IClock clock) : ICommandHandler<RemoveTrainingContractSignatoryCommand>
{
    public async Task<Result> Handle(RemoveTrainingContractSignatoryCommand command, CancellationToken ct)
    {
        Result<TrainingContract> loaded = await SignatoryLoader.LoadAsync(repository, command.OrganizationId, command.ContractId, ct);
        if (loaded.IsFailure) return Result.Failure(loaded.Error);
        Result result = loaded.Value.RemoveSignatory(command.SignatoryId); if (result.IsFailure) return result;
        loaded.Value.SetModifiedAudit(clock.UtcNow, command.ActorUserId); await uow.CommitAsync(ct); return Result.Success();
    }
}

public sealed class DecideTrainingContractSignatoryAuthorityCommandHandler(ITrainingContractRepository repository, IContractsUnitOfWork uow, IClock clock) : ICommandHandler<DecideTrainingContractSignatoryAuthorityCommand>
{
    public async Task<Result> Handle(DecideTrainingContractSignatoryAuthorityCommand command, CancellationToken ct)
    {
        Result<TrainingContract> loaded = await SignatoryLoader.LoadAsync(repository, command.OrganizationId, command.ContractId, ct);
        if (loaded.IsFailure) return Result.Failure(loaded.Error);
        DateTimeOffset now = clock.UtcNow;
        Result result = loaded.Value.DecideSignatoryAuthority(command.SignatoryId, command.Approved, command.Reason, command.ActorUserId, now);
        if (result.IsFailure) return result;
        loaded.Value.SetModifiedAudit(now, command.ActorUserId); await uow.CommitAsync(ct); return Result.Success();
    }
}
