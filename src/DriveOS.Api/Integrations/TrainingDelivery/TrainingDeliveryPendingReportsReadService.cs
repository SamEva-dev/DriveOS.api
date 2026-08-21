using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed record TrainingDeliveryPendingReportItem(
    Guid SessionId,
    Guid StudentId,
    string StudentDisplayName,
    Guid InstructorId,
    Guid? BranchId,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    DateTimeOffset ActualEndAtUtc,
    int ReportStatus,
    int ReportVersion,
    int LastCompletedStep,
    int CompletionPercent,
    int ResumeStep,
    DateTimeOffset? LastSavedAtUtc,
    DateTimeOffset DueAtUtc,
    bool IsOverdue,
    bool HasOpenIncident,
    bool HasCriticalIncident,
    bool IsRejectedForCorrection,
    bool IsWaitingForValidation,
    string? TrainingCategory);

internal sealed record TrainingDeliveryPendingReportsSummary(
    int Total,
    int Drafts,
    int ToComplete,
    int ToCorrect,
    int ToValidate,
    int Overdue);

internal sealed record TrainingDeliveryPendingReportsResponse(
    DateTimeOffset GeneratedAtUtc,
    bool IsPersonalScope,
    TrainingDeliveryPendingReportsSummary Summary,
    IReadOnlyCollection<TrainingDeliveryPendingReportItem> Items);

internal interface ITrainingDeliveryPendingReportsReadService
{
    Task<TrainingDeliveryPendingReportsResponse> GetAsync(
        OrganizationId organizationId,
        UserId currentUserId,
        bool canMonitorAll,
        bool mineOnly,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}

internal sealed class TrainingDeliveryPendingReportsReadService(
    TrainingDeliveryDbContext trainingDeliveryDb,
    StudentsDbContext studentsDb,
    IConfiguration configuration) : ITrainingDeliveryPendingReportsReadService
{
    // The functional specification defines an overdue category but no SLA value yet.
    // Keep a configurable technical default until an organization-level policy owns this value.
    private readonly TimeSpan _submissionDeadline = TimeSpan.FromHours(
        Math.Clamp(configuration.GetValue<int?>("TrainingDelivery:Reports:SubmissionDeadlineHours") ?? 24, 1, 720));

    public async Task<TrainingDeliveryPendingReportsResponse> GetAsync(
        OrganizationId organizationId,
        UserId currentUserId,
        bool canMonitorAll,
        bool mineOnly,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default)
    {
        bool personalScope = !canMonitorAll || mineOnly;

        List<PendingProjection> rows = await trainingDeliveryDb.TrainingSessions
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.Status == TrainingSessionStatus.Completed &&
                (!personalScope || x.InstructorId == currentUserId) &&
                (x.Report == null || x.Report.Status != SessionReportStatus.Validated))
            .OrderBy(x => x.ActualEndAtUtc ?? x.PlannedEndAtUtc)
            .Select(x => new PendingProjection(
                x.Id.Value,
                x.StudentId.Value,
                x.InstructorId.Value,
                x.BranchId == null ? null : x.BranchId.Value.Value,
                x.PlannedStartAtUtc,
                x.PlannedEndAtUtc,
                x.ActualEndAtUtc ?? x.PlannedEndAtUtc,
                x.Report == null ? -1 : (int)x.Report.Status,
                x.Report == null ? 0 : x.Report.Version,
                x.Report == null ? 0 : x.Report.LastCompletedStep,
                x.Report == null ? null : x.Report.LastSavedAtUtc,
                x.TrainingCategory))
            .ToListAsync(cancellationToken);

        Guid[] sessionIds = rows.Select(x => x.SessionId).ToArray();
        HashSet<Guid> openIncidentSessionIds = [];
        HashSet<Guid> criticalIncidentSessionIds = [];

        if (sessionIds.Length > 0)
        {
            var incidents = await trainingDeliveryDb.TrainingIncidents
                .AsNoTracking()
                .Where(x =>
                    x.OrganizationId == organizationId &&
                    sessionIds.Contains(x.TrainingSessionId.Value) &&
                    x.Status != TrainingIncidentStatus.Resolved &&
                    x.Status != TrainingIncidentStatus.Closed)
                .Select(x => new { SessionId = x.TrainingSessionId.Value, Severity = (int)x.Severity })
                .ToListAsync(cancellationToken);

            openIncidentSessionIds = incidents.Select(x => x.SessionId).ToHashSet();
            criticalIncidentSessionIds = incidents
                .Where(x => x.Severity == (int)TrainingIncidentSeverity.Critical)
                .Select(x => x.SessionId)
                .ToHashSet();
        }

        Guid[] studentIds = rows.Select(x => x.StudentId).Distinct().ToArray();
        Dictionary<Guid, string> studentNames = studentIds.Length == 0
            ? []
            : await studentsDb.Students
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId && studentIds.Contains(x.Id.Value))
                .Select(x => new { Id = x.Id.Value, x.FirstName, x.LastName })
                .ToDictionaryAsync(
                    x => x.Id,
                    x => string.Join(' ', new[] { x.FirstName, x.LastName }.Where(v => !string.IsNullOrWhiteSpace(v))),
                    cancellationToken);

        TrainingDeliveryPendingReportItem[] items = rows
            .Select(x =>
            {
                int completedStep = Math.Clamp(x.LastCompletedStep, 0, 9);
                int completionPercent = x.ReportStatus is (int)SessionReportStatus.ReadyToSubmit
                    or (int)SessionReportStatus.Submitted
                    or (int)SessionReportStatus.PendingSupervisorReview
                    or (int)SessionReportStatus.Validated
                    ? 100
                    : (int)Math.Round(completedStep / 9d * 100d, MidpointRounding.AwayFromZero);
                int resumeStep = x.ReportStatus == (int)SessionReportStatus.RejectedForCorrection
                    ? 9
                    : Math.Clamp(completedStep == 0 ? 1 : completedStep, 1, 9);
                DateTimeOffset dueAtUtc = x.ActualEndAtUtc + _submissionDeadline;
                return new TrainingDeliveryPendingReportItem(
                    x.SessionId,
                    x.StudentId,
                    ResolveStudentName(studentNames, x.StudentId),
                    x.InstructorId,
                    x.BranchId,
                    x.PlannedStartAtUtc,
                    x.PlannedEndAtUtc,
                    x.ActualEndAtUtc,
                    x.ReportStatus,
                    x.ReportVersion,
                    completedStep,
                    completionPercent,
                    resumeStep,
                    x.LastSavedAtUtc,
                    dueAtUtc,
                    dueAtUtc < nowUtc,
                    openIncidentSessionIds.Contains(x.SessionId),
                    criticalIncidentSessionIds.Contains(x.SessionId),
                    x.ReportStatus == (int)SessionReportStatus.RejectedForCorrection,
                    x.ReportStatus is (int)SessionReportStatus.Submitted or (int)SessionReportStatus.PendingSupervisorReview,
                    x.TrainingCategory);
            })
            .OrderByDescending(x => x.HasCriticalIncident)
            .ThenByDescending(x => x.IsOverdue)
            .ThenByDescending(x => x.IsRejectedForCorrection)
            .ThenBy(x => x.DueAtUtc)
            .ToArray();

        TrainingDeliveryPendingReportsSummary summary = new(
            items.Length,
            items.Count(x => x.ReportStatus == (int)SessionReportStatus.Draft),
            items.Count(x => x.ReportStatus < 0 || x.ReportStatus == (int)SessionReportStatus.Draft && x.CompletionPercent < 100),
            items.Count(x => x.IsRejectedForCorrection),
            items.Count(x => x.IsWaitingForValidation),
            items.Count(x => x.IsOverdue));

        return new TrainingDeliveryPendingReportsResponse(nowUtc, personalScope, summary, items);
    }

    private static string ResolveStudentName(IReadOnlyDictionary<Guid, string> names, Guid studentId) =>
        names.TryGetValue(studentId, out string? name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : studentId.ToString("N")[..8].ToUpperInvariant();

    private sealed record PendingProjection(
        Guid SessionId,
        Guid StudentId,
        Guid InstructorId,
        Guid? BranchId,
        DateTimeOffset PlannedStartAtUtc,
        DateTimeOffset PlannedEndAtUtc,
        DateTimeOffset ActualEndAtUtc,
        int ReportStatus,
        int ReportVersion,
        int LastCompletedStep,
        DateTimeOffset? LastSavedAtUtc,
        string? TrainingCategory);
}
