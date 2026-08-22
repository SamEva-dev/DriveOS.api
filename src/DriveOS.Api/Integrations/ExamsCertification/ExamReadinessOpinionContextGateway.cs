using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CurriculumPedagogy.Application.Readiness;
using DriveOS.Modules.ExamsCertification.Application.Readiness;
using DriveOS.Modules.ExamsCertification.Application.Readiness.Opinions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamReadinessOpinionContextGateway(
    IPedagogicalReadinessReadService pedagogy,
    IClock clock) : IExamReadinessOpinionContextGateway
{
    public async Task<Result<ExamReadinessOpinionContext>> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId trainingPathId,
        CancellationToken cancellationToken = default)
    {
        PedagogicalReadinessCheckResponse? readiness = await pedagogy.GetAsync(
            organizationId,
            trainingPathId,
            cancellationToken);

        if (readiness is null)
            return Result.Failure<ExamReadinessOpinionContext>(ExamReadinessApplicationErrors.TrainingPathNotFound);

        if (readiness.StudentId != studentId.Value)
            return Result.Failure<ExamReadinessOpinionContext>(ExamReadinessApplicationErrors.TrainingPathStudentMismatch);

        bool criticalCompetenciesValidated = readiness.RequiredCompetencies == readiness.EvaluatedRequiredCompetencies
            && !readiness.Blockers.Contains("RequiredCompetenciesNotAssessed", StringComparer.OrdinalIgnoreCase);

        return Result.Success(new ExamReadinessOpinionContext(
            studentId.Value,
            trainingPathId.Value,
            readiness.RequiredCoveragePercent,
            readiness.RequiredCompetencies,
            readiness.EvaluatedRequiredCompetencies,
            criticalCompetenciesValidated,
            readiness.HasCompletedPedagogicalReview,
            readiness.LatestDecision?.Decision,
            readiness.Blockers,
            clock.UtcNow));
    }
}
