using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class MaterializeTrainingSessionCommandHandler(ITrainingSessionRepository repository, IConfirmedBookingSessionSourceGateway sourceGateway, ITrainingSessionMaterializationLock materializationLock, ITrainingDeliveryUnitOfWork unitOfWork, IClock clock) : ICommandHandler<MaterializeTrainingSessionCommand, TrainingSessionId>
{
    public async Task<Result<TrainingSessionId>> Handle(MaterializeTrainingSessionCommand command, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await materializationLock.AcquireAsync(command.OrganizationId, command.BookingId, cancellationToken);
            TrainingSession? existing = await repository.GetBySourceBookingForUpdateAsync(command.OrganizationId, command.BookingId, cancellationToken);
            if (existing is not null)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Success(existing.Id);
            }

            Result<ConfirmedBookingSessionSource> sourceResult = await sourceGateway.GetAsync(command.OrganizationId, command.BookingId, cancellationToken);
            if (sourceResult.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionId>(sourceResult.Error);
            }

            ConfirmedBookingSessionSource source = sourceResult.Value;
            Result<TrainingSession> materialized = TrainingSession.Materialize(TrainingSessionId.New(), new TrainingSessionMaterialization(source.OrganizationId, source.StudentOwnerOrganizationId, source.PerformingOrganizationId, source.BookingId, source.StudentId, source.TrainingPathId, source.InstructorId, source.BranchId, source.VehicleId, source.PlannedStartAtUtc, source.PlannedEndAtUtc, source.TrainingCategory, source.Objectives, source.MeetingPoint, source.PricingReference, source.TrainingCreditAccountId, source.CreditQuantity, source.CreditReservationReference), command.ActorUserId, clock.UtcNow);
            if (materialized.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionId>(materialized.Error);
            }

            repository.Add(materialized.Value);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(materialized.Value.Id);
        }
        catch
        {
            if (unitOfWork.HasActiveTransaction) await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
