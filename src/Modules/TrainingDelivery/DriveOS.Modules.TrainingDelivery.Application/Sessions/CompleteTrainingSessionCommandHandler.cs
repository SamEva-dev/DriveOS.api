using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class CompleteTrainingSessionCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork,
    ITrainingSessionCompletionConsequenceStore consequenceStore,
    IClock clock) : ICommandHandler<CompleteTrainingSessionCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(CompleteTrainingSessionCommand command, CancellationToken cancellationToken)
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

            Result<SessionReport> result = session.Complete(
                command.OperationId,
                command.ActualEndAtUtc,
                command.Summary,
                command.ObjectivesWorked,
                command.ObjectivesAchieved,
                command.NextObjective,
                command.InstructorComments,
                command.ActorUserId,
                clock.UtcNow);

            if (result.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(result.Error);
            }

            SessionReport report = result.Value;
            var snapshot = new TrainingSessionCompletionSnapshot(
                session.OrganizationId,
                session.StudentOwnerOrganizationId,
                session.PerformingOrganizationId,
                session.Id,
                session.SourceBookingId,
                session.StudentId,
                session.TrainingPathId,
                session.ActualInstructorId ?? session.ReadyInstructorId ?? session.InstructorId,
                session.ActualVehicleId ?? session.ReadyVehicleId ?? session.VehicleId,
                session.ActualBranchId ?? session.ReadyBranchId ?? session.BranchId,
                session.TrainingCategory,
                session.ActualStartAtUtc!.Value,
                report.ActualEndAtUtc,
                report.DeliveredDurationMinutes,
                report.DistanceKilometers,
                session.StartEnergyLevelPercent,
                session.EndEnergyLevelPercent ?? session.LatestEnergyLevelPercent,
                session.FuelAddedLiters,
                session.ChargedEnergyKwh,
                session.PricingReference,
                session.TrainingCreditAccountId,
                session.CreditQuantity,
                session.CreditReservationReference,
                command.OperationId,
                command.ActorUserId,
                session.CompletedAtUtc ?? clock.UtcNow);
            await consequenceStore.EnqueueAsync(snapshot, cancellationToken);

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
