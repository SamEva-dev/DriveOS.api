using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class FinishTrainingSessionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    ITrainingSessionCompletionConsequenceStore consequenceStore,
    IClock clock) : ICommandHandler<FinishTrainingSessionCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(FinishTrainingSessionCommand command, CancellationToken cancellationToken)
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

            Result result = session.FinishExecution(command.OperationId, command.ActualEndAtUtc, command.EndEnergyLevelPercent, command.ActorUserId, clock.UtcNow);
            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(result.Error);
            }

            var snapshot = new TrainingSessionCompletionSnapshot(
                session.OrganizationId, session.StudentOwnerOrganizationId, session.PerformingOrganizationId, session.Id, session.SourceBookingId,
                session.StudentId, session.TrainingPathId, session.ActualInstructorId ?? session.ReadyInstructorId ?? session.InstructorId,
                session.ActualVehicleId ?? session.ReadyVehicleId ?? session.VehicleId, session.ActualBranchId ?? session.ReadyBranchId ?? session.BranchId,
                session.TrainingCategory, session.ActualStartAtUtc!.Value, session.ActualEndAtUtc!.Value, session.DeliveredDurationMinutes!.Value, session.DistanceKilometers,
                session.StartEnergyLevelPercent, session.EndEnergyLevelPercent ?? session.LatestEnergyLevelPercent, session.FuelAddedLiters, session.ChargedEnergyKwh,
                session.PricingReference, session.TrainingCreditAccountId, session.CreditQuantity, session.CreditReservationReference, command.OperationId,
                command.ActorUserId, session.CompletedAtUtc ?? clock.UtcNow);
            await consequenceStore.EnqueueAsync(snapshot, cancellationToken);

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
