using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class SaveTrainingSessionReportDraftCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SaveTrainingSessionReportDraftCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(SaveTrainingSessionReportDraftCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.NotFound);
            }

            Result<SessionReport> result = session.SaveReportDraft(
                command.OperationId,
                command.ExpectedVersion,
                command.LastCompletedStep,
                command.Summary,
                command.ObjectivesWorked,
                command.ObjectivesAchieved,
                command.NextObjective,
                command.SharedComment ?? session.Report?.SharedComment,
                command.InternalNote ?? session.Report?.InternalNote,
                command.ActorUserId,
                clock.UtcNow);

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
            if (unitOfWork.HasActiveTransaction)
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
