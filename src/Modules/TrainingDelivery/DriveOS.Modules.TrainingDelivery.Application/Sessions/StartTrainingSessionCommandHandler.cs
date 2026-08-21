using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class StartTrainingSessionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionReadinessGateway readinessGateway,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock,
    TrainingSessionExecutionOptions options)
    : ICommandHandler<StartTrainingSessionCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(StartTrainingSessionCommand command, CancellationToken cancellationToken)
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

            // A retry after a successful commit must match the persisted operation id and payload.
            // This is required by the field/offline replay contract from SCR-SES-020.
            if (session.Status == TrainingSessionStatus.InProgress)
            {
                Result replay = session.ValidateStartReplay(command.OperationId, command.StartedAtUtc, command.ActorUserId);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return replay.IsSuccess
                    ? Result.Success(TrainingSessionMappings.ToResponse(session))
                    : Result.Failure<TrainingSessionResponse>(replay.Error);
            }

            Result<TrainingSessionExecutionReadiness> readinessResult = await readinessGateway.CheckAsync(command.OrganizationId, session.SourceBookingId, cancellationToken);
            if (readinessResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(readinessResult.Error);
            }

            DateTimeOffset serverNow = clock.UtcNow;
            if (command.StartedAtUtc.ToUniversalTime() > serverNow.AddMinutes(5))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.StartTimestampInvalid);
            }

            TrainingSessionExecutionReadiness readiness = readinessResult.Value;
            if (!readiness.IsReady)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(TrainingSessionErrors.ResourcesNotReady);
            }

            Result startResult = session.Start(
                command.OperationId,
                new TrainingSessionReadinessSnapshot(readiness.IsReady, readiness.InstructorId, readiness.BranchId, readiness.VehicleId, readiness.PlannedStartAtUtc, readiness.PlannedEndAtUtc),
                command.ActorUserId,
                command.StartedAtUtc,
                options.StartEarlyToleranceMinutes,
                options.StartLateToleranceMinutes,
                options.ReadinessValidityMinutes);

            if (startResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(startResult.Error);
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
