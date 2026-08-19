using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CurriculumPedagogy.Application.PedagogicalReviews;
using DriveOS.Modules.CurriculumPedagogy.Application.Progression;
using DriveOS.Modules.CurriculumPedagogy.Application.Readiness;
using DriveOS.Modules.CurriculumPedagogy.Application.RemediationPlans;
using DriveOS.Modules.CurriculumPedagogy.Application.StudentOverview;
using DriveOS.Modules.CurriculumPedagogy.Application.TrainingPaths;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class StudentPedagogyOverviewReadService(
    ITrainingPathStudentGateway students,
    ITrainingPathReadService trainingPaths,
    IPedagogicalProgressionReadService progression,
    IPedagogicalReviewReadService reviews,
    IRemediationPlanReadService remediationPlans,
    IPedagogicalReadinessReadService readiness,
    IClock clock) : IStudentPedagogyOverviewReadService
{
    public async Task<StudentPedagogyOverviewResponse?> GetAsync(
        OrganizationId organizationId,
        PersonId studentId,
        TrainingPathId? requestedTrainingPathId = null,
        CancellationToken cancellationToken = default)
    {
        if (!await students.ExistsAsync(organizationId, studentId, cancellationToken))
            return null;

        IReadOnlyCollection<TrainingPathListItem> pathList =
            await trainingPaths.ListForStudentAsync(organizationId, studentId, cancellationToken);

        if (pathList.Count == 0)
        {
            return new StudentPedagogyOverviewResponse(
                studentId.Value,
                null,
                pathList,
                null,
                null,
                null,
                null,
                [],
                [],
                [new StudentPedagogyAlertResponse("Pedagogy.NoTrainingPath", "info")],
                clock.UtcNow);
        }

        TrainingPathListItem? selected = requestedTrainingPathId is { } requested
            ? pathList.FirstOrDefault(x => x.Id == requested.Value)
            : SelectCurrent(pathList);

        // A requested path must belong to the student and current tenant.
        if (selected is null)
            return null;

        var pathId = new TrainingPathId(selected.Id);
        TrainingPathDetailResponse? path = await trainingPaths.GetAsync(organizationId, pathId, cancellationToken);
        if (path is null || path.StudentId != studentId.Value)
            return null;

        Task<PedagogicalProgressionOverviewResponse?> progressionTask =
            progression.GetOverviewAsync(organizationId, pathId, includeInternalComments: false, recentTimelineLimit: 25, cancellationToken: cancellationToken);
        Task<IReadOnlyCollection<PedagogicalReviewResponse>> reviewsTask =
            reviews.ListForTrainingPathAsync(organizationId, pathId, cancellationToken);
        Task<IReadOnlyCollection<RemediationPlanResponse>> remediationTask =
            remediationPlans.ListAsync(organizationId, pathId, cancellationToken);
        Task<PedagogicalReadinessCheckResponse?> readinessTask =
            readiness.GetAsync(organizationId, pathId, cancellationToken);

        await Task.WhenAll(progressionTask, reviewsTask, remediationTask, readinessTask);

        PedagogicalProgressionOverviewResponse? progressionResult = await progressionTask;
        IReadOnlyCollection<PedagogicalReviewResponse> reviewResults = await reviewsTask;
        IReadOnlyCollection<RemediationPlanResponse> remediationResults = await remediationTask;
        PedagogicalReadinessCheckResponse? readinessResult = await readinessTask;

        int openRemediations = remediationResults.Count(x => x.Status is "Draft" or "Active");
        int completedReviews = reviewResults.Count(x => x.Status == "Completed");

        StudentPedagogyKpiResponse? kpis = progressionResult is null ? null : new(
            progressionResult.TotalCompetencies,
            progressionResult.RequiredCompetencies,
            progressionResult.AssessedCompetencies,
            progressionResult.UnassessedCompetencies,
            progressionResult.AssessmentCoveragePercent,
            progressionResult.RequiredAssessmentCoveragePercent,
            progressionResult.AssessmentCount,
            progressionResult.LevelChangeCount,
            openRemediations,
            completedReviews);

        IReadOnlyCollection<StudentPedagogyAlertResponse> alerts = BuildAlerts(
            path,
            progressionResult,
            reviewResults,
            remediationResults,
            readinessResult);

        return new StudentPedagogyOverviewResponse(
            studentId.Value,
            path.Id,
            pathList,
            path,
            kpis,
            progressionResult,
            readinessResult,
            reviewResults.OrderByDescending(x => x.RequestedAtUtc).ToArray(),
            remediationResults.OrderByDescending(x => x.PlanCreatedAtUtc).ToArray(),
            alerts,
            clock.UtcNow);
    }

    private static TrainingPathListItem SelectCurrent(IReadOnlyCollection<TrainingPathListItem> paths)
    {
        static int Priority(string status) => status switch
        {
            "Active" => 0,
            "Suspended" => 1,
            "ReadyForActivation" => 2,
            "Draft" => 3,
            "Completed" => 4,
            "Cancelled" => 5,
            _ => 6
        };

        return paths
            .OrderBy(x => Priority(x.Status))
            .ThenByDescending(x => x.CreatedAtUtc)
            .First();
    }

    private IReadOnlyCollection<StudentPedagogyAlertResponse> BuildAlerts(
        TrainingPathDetailResponse path,
        PedagogicalProgressionOverviewResponse? progression,
        IReadOnlyCollection<PedagogicalReviewResponse> reviews,
        IReadOnlyCollection<RemediationPlanResponse> remediationPlans,
        PedagogicalReadinessCheckResponse? readiness)
    {
        var alerts = new List<StudentPedagogyAlertResponse>();

        if (path.Status == "Suspended")
            alerts.Add(new("Pedagogy.TrainingPathSuspended", "warning", path.Id));

        if (progression is { TotalCompetencies: > 0, AssessmentCount: 0 })
            alerts.Add(new("Pedagogy.NoAssessmentRecorded", "info", path.Id));

        if (readiness is { RequiredCompetencies: > 0 } && readiness.EvaluatedRequiredCompetencies < readiness.RequiredCompetencies)
            alerts.Add(new("Pedagogy.RequiredCompetenciesPending", "info", path.Id));

        foreach (RemediationPlanResponse plan in remediationPlans.Where(x => x.Status is "Draft" or "Active"))
        {
            string severity = plan.ReviewDate < DateOnly.FromDateTime(clock.UtcNow.UtcDateTime) ? "warning" : "info";
            alerts.Add(new("Pedagogy.OpenRemediationPlan", severity, plan.Id));
        }

        if (!reviews.Any(x => x.Status == "Completed") && (path.Status is "Active" or "Suspended"))
            alerts.Add(new("Pedagogy.NoCompletedReview", "info", path.Id));

        if (readiness is { Recommendation: "Blocked" })
            alerts.Add(new("Pedagogy.ReadinessBlocked", "warning", path.Id));
        else if (readiness is { Recommendation: "EligibleForHumanDecision", LatestDecision: null })
            alerts.Add(new("Pedagogy.ReadinessDecisionRequired", "info", path.Id));

        return alerts;
    }
}
