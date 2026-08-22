using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed record TrainingDeliveryDashboardKpis(
    int SessionsToday,
    int InProgress,
    int Completed,
    int MissingReports,
    int LateStarts,
    int Absences,
    int Cancelled,
    int OpenIncidents,
    int DurationsToValidate,
    int? SyncFailures);

internal sealed record TrainingDeliveryDashboardSession(
    Guid Id,
    Guid StudentId,
    string StudentDisplayName,
    Guid InstructorId,
    Guid? VehicleId,
    Guid? BranchId,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    DateTimeOffset? ActualStartAtUtc,
    DateTimeOffset? ActualEndAtUtc,
    int? DeliveredDurationMinutes,
    int Status,
    int? AttendanceStatus,
    string? TrainingCategory,
    string? Objectives,
    string? MeetingPoint,
    bool HasReport,
    int AssessmentCount,
    bool HasOpenIncident,
    bool HasCriticalIncident);

internal sealed record TrainingDeliveryDashboardIncident(
    Guid Id,
    Guid TrainingSessionId,
    Guid StudentId,
    string StudentDisplayName,
    int IncidentType,
    int Severity,
    int Status,
    DateTimeOffset OccurredAtUtc,
    string Description,
    bool EscalationRequired);

internal sealed record TrainingDeliveryDashboardResponse(
    DateTimeOffset WindowStartAtUtc,
    DateTimeOffset WindowEndAtUtc,
    DateTimeOffset GeneratedAtUtc,
    TrainingDeliveryDashboardKpis Kpis,
    IReadOnlyCollection<TrainingDeliveryDashboardSession> Sessions,
    IReadOnlyCollection<TrainingDeliveryDashboardIncident> Incidents);

internal interface ITrainingDeliveryDashboardReadService
{
    Task<TrainingDeliveryDashboardResponse> GetAsync(
        OrganizationId organizationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        DateTimeOffset nowUtc,
        UserId? instructorId = null,
        CancellationToken cancellationToken = default);
}

internal sealed class TrainingDeliveryDashboardReadService(
    TrainingDeliveryDbContext trainingDeliveryDb,
    StudentsDbContext studentsDb) : ITrainingDeliveryDashboardReadService
{
    private const int DurationVarianceThresholdMinutes = 15;

    public async Task<TrainingDeliveryDashboardResponse> GetAsync(
        OrganizationId organizationId,
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        DateTimeOffset nowUtc,
        UserId? instructorId = null,
        CancellationToken cancellationToken = default)
    {
        if (endAtUtc <= startAtUtc)
            throw new ArgumentOutOfRangeException(nameof(endAtUtc), "The dashboard window end must be after its start.");

        List<SessionProjection> sessionRows = await trainingDeliveryDb.TrainingSessions
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId &&
                (!instructorId.HasValue || x.InstructorId == instructorId.Value) &&
                x.PlannedStartAtUtc >= startAtUtc &&
                x.PlannedStartAtUtc < endAtUtc)
            .OrderBy(x => x.PlannedStartAtUtc)
            .Select(x => new SessionProjection(
                x.Id.Value,
                x.StudentId.Value,
                x.InstructorId.Value,
                x.VehicleId,
                x.BranchId == null ? null : x.BranchId.Value.Value,
                x.PlannedStartAtUtc,
                x.PlannedEndAtUtc,
                x.ActualStartAtUtc,
                x.ActualEndAtUtc,
                x.DeliveredDurationMinutes,
                (int)x.Status,
                !x.CurrentAttendanceId.HasValue
                    ? null
                    : x.AttendanceHistory
                        .Where(a => a.Id == x.CurrentAttendanceId.Value)
                        .Select(a => (int?)a.Status)
                        .SingleOrDefault(),
                x.TrainingCategory,
                x.Objectives,
                x.MeetingPoint,
                x.Report != null,
                x.CompetencyAssessments.Count))
            .ToListAsync(cancellationToken);

        Guid[] visibleSessionIds = sessionRows.Select(x => x.Id).ToArray();

        IQueryable<TrainingIncident> incidentQuery = trainingDeliveryDb.TrainingIncidents
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.Status != TrainingIncidentStatus.Resolved &&
                x.Status != TrainingIncidentStatus.Closed);

        if (instructorId.HasValue)
        {
            TrainingSessionId[] visibleTypedSessionIds = visibleSessionIds
                .Select(static id => new TrainingSessionId(id))
                .ToArray();

            incidentQuery = WhereStrongIdIn(
                incidentQuery,
                x => x.TrainingSessionId,
                visibleTypedSessionIds);
        }

        List<IncidentProjection> incidentRows = await incidentQuery
            .OrderByDescending(x => x.Severity)
            .ThenByDescending(x => x.OccurredAtUtc)
            .Select(x => new IncidentProjection(
                x.Id.Value,
                x.TrainingSessionId.Value,
                x.StudentId.Value,
                (int)x.IncidentType,
                (int)x.Severity,
                (int)x.Status,
                x.OccurredAtUtc,
                x.Description,
                x.EscalationRequired))
            .ToListAsync(cancellationToken);

        Guid[] studentIds = sessionRows.Select(x => x.StudentId)
            .Concat(incidentRows.Select(x => x.StudentId))
            .Distinct()
            .ToArray();

        Dictionary<Guid, string> studentNames;
        if (studentIds.Length == 0)
        {
            studentNames = [];
        }
        else
        {
            PersonId[] typedStudentIds = studentIds.Select(static id => new PersonId(id)).ToArray();
            var studentQuery = WhereStrongIdIn(
                studentsDb.Students.AsNoTracking().Where(x => x.OrganizationId == organizationId),
                x => x.Id,
                typedStudentIds);

            studentNames = await studentQuery
                .Select(x => new { Id = x.Id.Value, x.FirstName, x.LastName })
                .ToDictionaryAsync(
                    x => x.Id,
                    x => string.Join(' ', new[] { x.FirstName, x.LastName }.Where(v => !string.IsNullOrWhiteSpace(v))),
                    cancellationToken);
        }

        HashSet<Guid> sessionsWithOpenIncident = incidentRows.Select(x => x.TrainingSessionId).ToHashSet();
        HashSet<Guid> sessionsWithCriticalIncident = incidentRows
            .Where(x => x.Severity == (int)TrainingIncidentSeverity.Critical)
            .Select(x => x.TrainingSessionId)
            .ToHashSet();

        TrainingDeliveryDashboardSession[] sessions = sessionRows
            .Select(x => new TrainingDeliveryDashboardSession(
                x.Id,
                x.StudentId,
                ResolveStudentName(studentNames, x.StudentId),
                x.InstructorId,
                x.VehicleId,
                x.BranchId,
                x.PlannedStartAtUtc,
                x.PlannedEndAtUtc,
                x.ActualStartAtUtc,
                x.ActualEndAtUtc,
                x.DeliveredDurationMinutes,
                x.Status,
                x.AttendanceStatus,
                x.TrainingCategory,
                x.Objectives,
                x.MeetingPoint,
                x.HasReport,
                x.AssessmentCount,
                sessionsWithOpenIncident.Contains(x.Id),
                sessionsWithCriticalIncident.Contains(x.Id)))
            .ToArray();

        TrainingDeliveryDashboardIncident[] incidents = incidentRows
            .Select(x => new TrainingDeliveryDashboardIncident(
                x.Id,
                x.TrainingSessionId,
                x.StudentId,
                ResolveStudentName(studentNames, x.StudentId),
                x.IncidentType,
                x.Severity,
                x.Status,
                x.OccurredAtUtc,
                x.Description,
                x.EscalationRequired))
            .ToArray();

        int missingReports = sessions.Count(x =>
            x.Status == (int)TrainingSessionStatus.Completed && !x.HasReport);
        int lateStarts = sessions.Count(x =>
            x.PlannedStartAtUtc < nowUtc &&
            x.Status is (int)TrainingSessionStatus.Scheduled or (int)TrainingSessionStatus.Ready);
        int absences = sessions.Count(x => IsAbsence(x.AttendanceStatus));
        int durationsToValidate = sessions.Count(HasMaterialDurationVariance);

        TrainingDeliveryDashboardKpis kpis = new(
            sessions.Length,
            sessions.Count(x => x.Status is (int)TrainingSessionStatus.InProgress or (int)TrainingSessionStatus.Interrupted),
            sessions.Count(x => x.Status == (int)TrainingSessionStatus.Completed),
            missingReports,
            lateStarts,
            absences,
            sessions.Count(x => x.Status == (int)TrainingSessionStatus.Cancelled),
            incidents.Length,
            durationsToValidate,
            null);

        return new TrainingDeliveryDashboardResponse(
            startAtUtc,
            endAtUtc,
            nowUtc,
            kpis,
            sessions,
            incidents);
    }

    private static IQueryable<TEntity> WhereStrongIdIn<TEntity, TId>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, TId>> selector,
        IReadOnlyCollection<TId> values)
    {
        if (values.Count == 0)
            return query.Where(_ => false);

        Expression body = Expression.Constant(false);
        foreach (TId value in values)
        {
            body = Expression.OrElse(
                body,
                Expression.Equal(selector.Body, Expression.Constant(value, typeof(TId))));
        }

        return query.Where(Expression.Lambda<Func<TEntity, bool>>(body, selector.Parameters));
    }

    private static string ResolveStudentName(IReadOnlyDictionary<Guid, string> names, Guid studentId) =>
        names.TryGetValue(studentId, out string? name) && !string.IsNullOrWhiteSpace(name)
            ? name
            : studentId.ToString("N")[..8].ToUpperInvariant();

    private static bool IsAbsence(int? status) => status is
        (int)TrainingSessionAttendanceStatus.StudentAbsent or
        (int)TrainingSessionAttendanceStatus.InstructorAbsent or
        (int)TrainingSessionAttendanceStatus.ExcusedAbsence or
        (int)TrainingSessionAttendanceStatus.UnexcusedAbsence or
        (int)TrainingSessionAttendanceStatus.UnableToDeliver;

    private static bool HasMaterialDurationVariance(TrainingDeliveryDashboardSession session)
    {
        if (session.Status != (int)TrainingSessionStatus.Completed || session.DeliveredDurationMinutes is null)
            return false;

        double plannedMinutes = (session.PlannedEndAtUtc - session.PlannedStartAtUtc).TotalMinutes;
        return Math.Abs(session.DeliveredDurationMinutes.Value - plannedMinutes) >= DurationVarianceThresholdMinutes;
    }

    private sealed record SessionProjection(
        Guid Id,
        Guid StudentId,
        Guid InstructorId,
        Guid? VehicleId,
        Guid? BranchId,
        DateTimeOffset PlannedStartAtUtc,
        DateTimeOffset PlannedEndAtUtc,
        DateTimeOffset? ActualStartAtUtc,
        DateTimeOffset? ActualEndAtUtc,
        int? DeliveredDurationMinutes,
        int Status,
        int? AttendanceStatus,
        string? TrainingCategory,
        string? Objectives,
        string? MeetingPoint,
        bool HasReport,
        int AssessmentCount);

    private sealed record IncidentProjection(
        Guid Id,
        Guid TrainingSessionId,
        Guid StudentId,
        int IncidentType,
        int Severity,
        int Status,
        DateTimeOffset OccurredAtUtc,
        string Description,
        bool EscalationRequired);
}
