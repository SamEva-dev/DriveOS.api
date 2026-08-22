using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Readiness;

public sealed class RecordExamReadinessDecisionCommandHandler(
    IExamReadinessDecisionRepository repository,
    IExamsCertificationUnitOfWork unitOfWork,
    IExamReadinessSnapshotGateway snapshotGateway,
    IClock clock) : ICommandHandler<RecordExamReadinessDecisionCommand, ExamReadinessDecisionId>
{
    public async Task<Result<ExamReadinessDecisionId>> Handle(
        RecordExamReadinessDecisionCommand command,
        CancellationToken cancellationToken)
    {
        Result<ExamReadinessSnapshot> snapshotResult = await snapshotGateway.EvaluateAsync(
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            cancellationToken);

        if (snapshotResult.IsFailure)
            return Result.Failure<ExamReadinessDecisionId>(snapshotResult.Error);

        ExamReadinessSnapshot snapshot = snapshotResult.Value;

        ExamReadinessDecision? current = await repository.GetCurrentForUpdateAsync(
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            cancellationToken);

        int nextVersion = (current?.Version ?? 0) + 1;
        ExamReadinessDecisionId newDecisionId = ExamReadinessDecisionId.New();

        Result<ExamReadinessDecision> creation = ExamReadinessDecision.Record(
            newDecisionId,
            command.OrganizationId,
            command.StudentId,
            command.TrainingPathId,
            nextVersion,
            command.Outcome,
            snapshot.PedagogicalCheck,
            snapshot.AdministrativeCheck,
            snapshot.FinancialCheck,
            snapshot.RegulatoryCheck,
            command.Rationale,
            command.Conditions,
            command.ReviewerId,
            clock.UtcNow);

        if (creation.IsFailure)
            return Result.Failure<ExamReadinessDecisionId>(creation.Error);

        if (current is not null)
        {
            Result supersede = current.Supersede(newDecisionId, clock.UtcNow, command.ReviewerId);
            if (supersede.IsFailure)
                return Result.Failure<ExamReadinessDecisionId>(supersede.Error);
        }

        creation.Value.SetCreatedAudit(clock.UtcNow, command.ReviewerId);
        repository.Add(creation.Value);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(newDecisionId);
    }
}
