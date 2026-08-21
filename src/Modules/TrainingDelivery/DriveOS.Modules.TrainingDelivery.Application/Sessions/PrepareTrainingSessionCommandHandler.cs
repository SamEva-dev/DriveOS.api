using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class PrepareTrainingSessionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionReadinessGateway readinessGateway,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock,
    TrainingSessionExecutionOptions options)
    : ICommandHandler<PrepareTrainingSessionCommand, TrainingSessionPreparationResponse>
{
    public async Task<Result<TrainingSessionPreparationResponse>> Handle(PrepareTrainingSessionCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            TrainingSession? session = await repository.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionPreparationResponse>(TrainingSessionErrors.NotFound);
            }

            Result<TrainingSessionExecutionReadiness> readinessResult = await readinessGateway.CheckAsync(command.OrganizationId, session.SourceBookingId, cancellationToken);
            if (readinessResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionPreparationResponse>(readinessResult.Error);
            }

            TrainingSessionExecutionReadiness readiness = readinessResult.Value;
            if (!readiness.IsReady)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(TrainingSessionMappings.ToPreparationResponse(session, readiness, clock.UtcNow));
            }

            Result readyResult = session.MarkReady(
                new TrainingSessionReadinessSnapshot(readiness.IsReady, readiness.InstructorId, readiness.BranchId, readiness.VehicleId, readiness.PlannedStartAtUtc, readiness.PlannedEndAtUtc),
                command.ActorUserId,
                clock.UtcNow,
                options.PreparationLeadMinutes);

            if (readyResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionPreparationResponse>(readyResult.Error);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(TrainingSessionMappings.ToPreparationResponse(session, readiness, session.ReadinessCheckedAtUtc ?? clock.UtcNow));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
