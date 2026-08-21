using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class UpdateTrainingSessionSharedCommentCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UpdateTrainingSessionSharedCommentCommand, TrainingSessionResponse>
{
    public Task<Result<TrainingSessionResponse>> Handle(UpdateTrainingSessionSharedCommentCommand command, CancellationToken cancellationToken) =>
        TrainingSessionNarrativeHandler.Update(repository, executionLock, unitOfWork, clock, command.OrganizationId, command.SessionId,
            command.OperationId, command.ExpectedVersion, command.Content, false, command.ActorUserId, cancellationToken);
}

public sealed class UpdateTrainingSessionInternalNoteCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UpdateTrainingSessionInternalNoteCommand, TrainingSessionResponse>
{
    public Task<Result<TrainingSessionResponse>> Handle(UpdateTrainingSessionInternalNoteCommand command, CancellationToken cancellationToken) =>
        TrainingSessionNarrativeHandler.Update(repository, executionLock, unitOfWork, clock, command.OrganizationId, command.SessionId,
            command.OperationId, command.ExpectedVersion, command.Content, true, command.ActorUserId, cancellationToken);
}

internal static class TrainingSessionNarrativeHandler
{
    internal static async Task<Result<TrainingSessionResponse>> Update(
        ITrainingSessionRepository repository,
        ITrainingSessionExecutionLock executionLock,
        ITrainingDeliveryUnitOfWork unitOfWork,
        IClock clock,
        DriveOS.SharedKernel.Identifiers.OrganizationId organizationId,
        DriveOS.SharedKernel.Identifiers.TrainingSessionId sessionId,
        Guid operationId,
        int expectedVersion,
        string? content,
        bool internalNote,
        DriveOS.SharedKernel.Identifiers.UserId actor,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(organizationId, sessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(organizationId, sessionId, cancellationToken);
            if (session is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound);
            }

            Result result = internalNote
                ? session.UpdateInternalNote(operationId, expectedVersion, content, actor, clock.UtcNow)
                : session.UpdateSharedComment(operationId, expectedVersion, content, actor, clock.UtcNow);

            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(result.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToResponse(session));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
