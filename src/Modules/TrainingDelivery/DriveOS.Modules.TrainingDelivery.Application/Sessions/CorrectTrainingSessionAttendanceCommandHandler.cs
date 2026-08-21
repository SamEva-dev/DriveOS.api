using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class CorrectTrainingSessionAttendanceCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock,
    TrainingSessionAttendanceOptions options)
    : ICommandHandler<CorrectTrainingSessionAttendanceCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(CorrectTrainingSessionAttendanceCommand command, CancellationToken cancellationToken)
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

            Result<SessionAttendance> result = session.CorrectAttendance(
                command.OperationId,
                command.Status,
                command.ActualArrivalAtUtc,
                command.ActualDepartureAtUtc,
                command.Reason,
                command.EvidenceDocumentId,
                command.ActorUserId,
                clock.UtcNow,
                options.CorrectionWindowHours,
                command.IsOverride,
                command.OverrideReason);

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
