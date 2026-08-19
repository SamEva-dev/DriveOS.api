using DriveOS.Modules.CurriculumPedagogy.Application.PedagogicalReviews;
using DriveOS.Modules.CurriculumPedagogy.Application.Progression;
using DriveOS.Modules.CurriculumPedagogy.Application.Readiness;
using DriveOS.Modules.CurriculumPedagogy.Application.RemediationPlans;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Application.StudentOverview;

public sealed record StudentPedagogyAlertResponse(
    string Code,
    string Severity,
    Guid? RelatedEntityId = null);

public sealed record StudentPedagogyKpiResponse(
    int TotalCompetencies,
    int RequiredCompetencies,
    int AssessedCompetencies,
    int UnassessedCompetencies,
    decimal AssessmentCoveragePercent,
    decimal RequiredAssessmentCoveragePercent,
    int AssessmentCount,
    int LevelChangeCount,
    int OpenRemediationPlans,
    int CompletedReviews);

public sealed record StudentPedagogyOverviewResponse(
    Guid StudentId,
    Guid? SelectedTrainingPathId,
    IReadOnlyCollection<TrainingPathListItem> TrainingPaths,
    TrainingPathDetailResponse? TrainingPath,
    StudentPedagogyKpiResponse? Kpis,
    PedagogicalProgressionOverviewResponse? Progression,
    PedagogicalReadinessCheckResponse? Readiness,
    IReadOnlyCollection<PedagogicalReviewResponse> Reviews,
    IReadOnlyCollection<RemediationPlanResponse> RemediationPlans,
    IReadOnlyCollection<StudentPedagogyAlertResponse> Alerts,
    DateTimeOffset GeneratedAtUtc);

public interface IStudentPedagogyOverviewReadService
{
    Task<StudentPedagogyOverviewResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId? requestedTrainingPathId = null,
        CancellationToken cancellationToken = default);
}
