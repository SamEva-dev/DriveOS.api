using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.Sessions;

public sealed class RecordTrainingSessionCompetencyAssessmentCommandHandler(
    ITrainingSessionRepository repository,
    ITrainingSessionExecutionLock executionLock,
    ITrainingSessionPedagogyGateway pedagogyGateway,
    ITrainingDeliveryUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecordTrainingSessionCompetencyAssessmentCommand, TrainingSessionResponse>
{
    public async Task<Result<TrainingSessionResponse>> Handle(RecordTrainingSessionCompetencyAssessmentCommand command, CancellationToken cancellationToken)
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

            Result<TrainingSessionPedagogyAssessmentReference> pedagogy = await pedagogyGateway.RecordAssessmentAsync(
                new TrainingSessionPedagogyAssessmentRequest(
                    command.OrganizationId,
                    session.TrainingPathId,
                    session.Id,
                    command.OperationId,
                    command.CompetencyId,
                    command.LevelCode,
                    command.InternalComment,
                    command.SharedComment,
                    command.AssessedAtUtc,
                    command.ActorUserId),
                cancellationToken);

            if (pedagogy.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(pedagogy.Error);
            }

            Result<SessionCompetencyAssessment> recorded = session.RecordCompetencyAssessment(
                command.OperationId,
                command.CompetencyId,
                pedagogy.Value.CurriculumVersionId,
                pedagogy.Value.PedagogyAssessmentId,
                command.LevelCode,
                command.ObservedCriteria,
                command.Context,
                command.RelatedInterventionId,
                command.InternalComment,
                command.SharedComment,
                command.EvidenceDocumentId,
                command.AssessedAtUtc,
                command.ActorUserId,
                clock.UtcNow);

            if (recorded.IsFailure)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<TrainingSessionResponse>(recorded.Error);
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
