using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Cancellations;

public sealed class CancelTrainingSessionCommandHandler(
    ITrainingSessionRepository sessions, ITrainingSessionCancellationRepository cancellations, ITrainingSessionExecutionLock executionLock,
    ITrainingDeliveryUnitOfWork unitOfWork, ITrainingSessionCancellationConsequenceStore consequenceStore, IClock clock)
    : ICommandHandler<CancelTrainingSessionCommand, SessionCancellationResponse>
{
    public async Task<Result<SessionCancellationResponse>> Handle(CancelTrainingSessionCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await executionLock.AcquireAsync(command.OrganizationId, command.SessionId, cancellationToken);
            SessionCancellation? existing = await cancellations.GetBySessionForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (existing is not null)
            {
                bool same = existing.Matches(command.OperationId, command.CancelledAtUtc, command.Reason, command.ReasonDetails, command.BillingDecision,
                    command.CreditDecision, command.PartialCreditQuantity, command.ProviderCompensationDecision, command.DecisionReason);
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return same ? Result.Success(SessionCancellationMappings.ToResponse(existing)) : Result.Failure<SessionCancellationResponse>(SessionCancellationErrors.OperationConflict);
            }

            TrainingSession? session = await sessions.GetByIdForUpdateAsync(command.OrganizationId, command.SessionId, cancellationToken);
            if (session is null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<SessionCancellationResponse>(TrainingSessionErrors.NotFound);
            }

            SessionCancellationId cancellationId = SessionCancellationId.New();
            Result<TrainingSessionCancellationFacts> stopped = session.CancelDuringExecution(cancellationId, command.CancelledAtUtc, command.ActorUserId, clock.UtcNow);
            if (stopped.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<SessionCancellationResponse>(stopped.Error);
            }

            TrainingSessionCancellationFacts f = stopped.Value;
            Result<SessionCancellation> created = SessionCancellation.Create(
                cancellationId, session.OrganizationId, session.Id, session.SourceBookingId, session.StudentOwnerOrganizationId, session.PerformingOrganizationId,
                session.StudentId, f.InstructorId, f.VehicleId, f.BranchId, f.ActualStartAtUtc, f.ActualEndAtUtc, f.GrossDurationMinutes,
                f.InterruptionDurationMinutes, f.DeliveredDurationMinutes, f.DistanceKilometers, command.Reason, command.ReasonDetails, command.BillingDecision,
                command.CreditDecision, command.PartialCreditQuantity, command.ProviderCompensationDecision, command.DecisionReason, session.TrainingCreditAccountId,
                session.CreditQuantity, session.CreditReservationReference, session.PricingReference, command.OperationId, command.ActorUserId, clock.UtcNow);
            if (created.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<SessionCancellationResponse>(created.Error);
            }

            cancellations.Add(created.Value);
            await consequenceStore.EnqueueAsync(created.Value, cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(SessionCancellationMappings.ToResponse(created.Value));
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
