using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CurriculumPedagogy.Application.Persistence;
using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;

internal static class TrainingPathLifecycleErrors
{
    internal static readonly Error NotFound = Error.NotFound(
        "CurriculumPedagogy.TrainingPath.NotFound",
        "errors.curriculumPedagogy.trainingPath.notFound");
}

public abstract class TrainingPathMutationHandlerBase(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
{
    protected async Task<Result<TrainingPath>> LoadAsync(
        OrganizationId organizationId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken)
    {
        TrainingPath? path = await trainingPaths.GetByIdForUpdateAsync(
            trainingPathId, organizationId, cancellationToken);
        return path is null
            ? Result.Failure<TrainingPath>(TrainingPathLifecycleErrors.NotFound)
            : Result.Success(path);
    }

    protected async Task<Result> CommitAsync(Result mutation, CancellationToken cancellationToken)
    {
        if (mutation.IsFailure) return mutation;
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

public sealed class MarkTrainingPathReadyCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<MarkTrainingPathReadyCommand>
{
    public async Task<Result> Handle(MarkTrainingPathReadyCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.MarkReadyForActivation(), cancellationToken);
    }
}

public sealed class ActivateTrainingPathCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork,
    IClock clock)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<ActivateTrainingPathCommand>
{
    public async Task<Result> Handle(ActivateTrainingPathCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Activate(command.ActorUserId, clock.UtcNow), cancellationToken);
    }
}

public sealed class SuspendTrainingPathCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork,
    IClock clock)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<SuspendTrainingPathCommand>
{
    public async Task<Result> Handle(SuspendTrainingPathCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Suspend(command.Reason, clock.UtcNow), cancellationToken);
    }
}

public sealed class ReactivateTrainingPathCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<ReactivateTrainingPathCommand>
{
    public async Task<Result> Handle(ReactivateTrainingPathCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Reactivate(), cancellationToken);
    }
}

public sealed class CompleteTrainingPathCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork,
    IClock clock)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<CompleteTrainingPathCommand>
{
    public async Task<Result> Handle(CompleteTrainingPathCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Complete(clock.UtcNow), cancellationToken);
    }
}

public sealed class CancelTrainingPathCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork,
    IClock clock)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<CancelTrainingPathCommand>
{
    public async Task<Result> Handle(CancelTrainingPathCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.Cancel(command.Reason, clock.UtcNow), cancellationToken);
    }
}

public sealed class AddTrainingPathMilestoneCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<AddTrainingPathMilestoneCommand, TrainingPathMilestoneId>
{
    public async Task<Result<TrainingPathMilestoneId>> Handle(AddTrainingPathMilestoneCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        if (loaded.IsFailure) return Result.Failure<TrainingPathMilestoneId>(loaded.Error);

        Result<TrainingPathMilestone> result = loaded.Value.AddMilestone(
            TrainingPathMilestoneId.New(), command.Code, command.Name, command.Description, command.Order, command.TargetDate);
        if (result.IsFailure) return Result.Failure<TrainingPathMilestoneId>(result.Error);

        //await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(result.Value.Id);
    }
}

public sealed class StartTrainingPathMilestoneCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<StartTrainingPathMilestoneCommand>
{
    public async Task<Result> Handle(StartTrainingPathMilestoneCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.StartMilestone(command.MilestoneId), cancellationToken);
    }
}

public sealed class CompleteTrainingPathMilestoneCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork,
    IClock clock)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<CompleteTrainingPathMilestoneCommand>
{
    public async Task<Result> Handle(CompleteTrainingPathMilestoneCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.CompleteMilestone(command.MilestoneId, command.ActorUserId, clock.UtcNow), cancellationToken);
    }
}

public sealed class CancelTrainingPathMilestoneCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICurriculumPedagogyUnitOfWork unitOfWork)
    : TrainingPathMutationHandlerBase(trainingPaths, unitOfWork), ICommandHandler<CancelTrainingPathMilestoneCommand>
{
    public async Task<Result> Handle(CancelTrainingPathMilestoneCommand command, CancellationToken cancellationToken)
    {
        Result<TrainingPath> loaded = await LoadAsync(command.OrganizationId, command.TrainingPathId, cancellationToken);
        return loaded.IsFailure ? Result.Failure(loaded.Error) : await CommitAsync(loaded.Value.CancelMilestone(command.MilestoneId), cancellationToken);
    }
}
