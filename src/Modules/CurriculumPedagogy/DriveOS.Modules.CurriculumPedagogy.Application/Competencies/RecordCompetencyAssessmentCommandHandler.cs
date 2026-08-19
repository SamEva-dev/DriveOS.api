using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CurriculumPedagogy.Application.Persistence;
using DriveOS.Modules.CurriculumPedagogy.Domain.Competencies;
using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.Competencies;

public sealed class RecordCompetencyAssessmentCommandHandler(
    ITrainingPathRepository trainingPaths,
    ICompetencyRecordRepository competencyRecords,
    ICurriculumCompetencyEligibilityService curriculumCompetencies,
    ICurriculumPedagogyUnitOfWork unitOfWork,
    IClock clock)
    : ICommandHandler<RecordCompetencyAssessmentCommand, CompetencyAssessmentId>
{
    public async Task<Result<CompetencyAssessmentId>> Handle(RecordCompetencyAssessmentCommand command, CancellationToken cancellationToken)
    {
        TrainingPath? path = await trainingPaths.GetByIdAsync(command.TrainingPathId, command.OrganizationId, cancellationToken);
        if (path is null)
            return Result.Failure<CompetencyAssessmentId>(RecordCompetencyAssessmentErrors.TrainingPathNotFound);
        if (path.Status != TrainingPathStatus.Active)
            return Result.Failure<CompetencyAssessmentId>(RecordCompetencyAssessmentErrors.TrainingPathNotActive);

        CurriculumCompetencyEligibility? competency = await curriculumCompetencies.GetAsync(
            command.OrganizationId, path.CurriculumVersionId, command.CompetencyId, cancellationToken);
        if (competency is null)
            return Result.Failure<CompetencyAssessmentId>(RecordCompetencyAssessmentErrors.CompetencyNotFound);

        CompetencyRecord? record = await competencyRecords.GetByTrainingPathAndCompetencyForUpdateAsync(
            command.OrganizationId, command.TrainingPathId, command.CompetencyId, cancellationToken);

        if (record is null)
        {
            Result<CompetencyRecord> create = CompetencyRecord.Create(
                CompetencyRecordId.New(), command.OrganizationId, command.TrainingPathId,
                path.CurriculumVersionId, command.CompetencyId, competency.IsRequired);
            if (create.IsFailure)
                return Result.Failure<CompetencyAssessmentId>(create.Error);
            record = create.Value;
            await competencyRecords.AddAsync(record, cancellationToken);
        }

        DateTimeOffset now = clock.UtcNow;
        DateTimeOffset assessedAt = command.AssessedAtUtc?.ToUniversalTime() ?? now;
        if (assessedAt > now.AddMinutes(5))
            return Result.Failure<CompetencyAssessmentId>(CompetencyRecordErrors.InvalidAssessment);

        CompetencyAssessmentId assessmentId = CompetencyAssessmentId.New();
        Result<CompetencyAssessment> assessment = record.RecordAssessment(
            assessmentId, command.LevelCode, command.AssessorUserId, command.SourceSessionId,
            command.Comment, command.IsVisibleToStudent, assessedAt, now);
        if (assessment.IsFailure)
            return Result.Failure<CompetencyAssessmentId>(assessment.Error);

        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(assessment.Value.Id);
    }
}
