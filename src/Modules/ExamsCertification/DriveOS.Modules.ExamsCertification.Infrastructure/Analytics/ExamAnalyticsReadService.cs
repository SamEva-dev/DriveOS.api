using DriveOS.Modules.ExamsCertification.Application.Analytics;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;
using System.Linq.Expressions;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Analytics;

/// <summary>
/// Transactional read-side analytics for BC-11. Metrics are derived from current authoritative state so
/// corrected/superseded results are reflected immediately. No mutable KPI projection is maintained here.
/// </summary>
internal sealed class ExamAnalyticsReadService(
    ExamsCertificationDbContext dbContext,
    IOptions<ExamAnalyticsOptions> options) : IExamAnalyticsReadService
{
    private readonly ExamAnalyticsOptions _options = options.Value;

    private sealed record AttemptRow(
        Guid AttemptId,
        Guid RegistrationId,
        DateTimeOffset ScheduledStartUtc,
        string ExamType,
        string LicenseCategory,
        Guid ExamCenterId,
        Guid? InstructorId,
        int AttemptNumber,
        ExamAttemptStatus Status,
        ExamAttendanceStatus AttendanceStatus);

    private sealed record ResultRow(
        Guid AttemptId,
        ExamResultOutcome Outcome,
        int AttemptNumber,
        string? FailureReasonCode);

    public async Task<ExamAnalyticsResponse> GetAsync(
        OrganizationId organizationId,
        ExamAnalyticsFilter filter,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset toUtc = (filter.ToUtc ?? DateTimeOffset.UtcNow).ToUniversalTime();
        DateTimeOffset fromUtc = (filter.FromUtc ?? toUtc.AddMonths(-_options.DefaultPeriodMonths)).ToUniversalTime();
        if (fromUtc > toUtc)
            (fromUtc, toUtc) = (toUtc, fromUtc);

        Guid[]? branchRegistrationIds = null;
        if (filter.BranchId is { } branchId)
        {
            var typedBranchId = new BranchId(branchId);
            branchRegistrationIds = await dbContext.ExamOperationalPlans
                .AsNoTracking()
                .Where(x => x.OrganizationId == organizationId && x.DepartureBranchId == typedBranchId)
                .Select(x => x.RegistrationId.Value)
                .Distinct()
                .ToArrayAsync(cancellationToken);
        }

        IQueryable<ExamAttempt> attemptQuery = dbContext.ExamAttempts
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.ScheduledStartUtc >= fromUtc
                && x.ScheduledStartUtc <= toUtc);

        if (branchRegistrationIds is not null)
        {
            ExamRegistrationId[] typedBranchRegistrationIds = branchRegistrationIds
                .Select(static id => new ExamRegistrationId(id))
                .ToArray();
            attemptQuery = WhereStrongIdIn(attemptQuery, x => x.RegistrationId, typedBranchRegistrationIds);
        }
        if (!string.IsNullOrWhiteSpace(filter.ExamType))
            attemptQuery = attemptQuery.Where(x => x.ExamType == filter.ExamType.Trim());
        if (!string.IsNullOrWhiteSpace(filter.LicenseCategory))
            attemptQuery = attemptQuery.Where(x => x.LicenseCategory == filter.LicenseCategory.Trim());
        if (filter.ExamCenterId is { } centerId)
            attemptQuery = attemptQuery.Where(x => x.ExamCenterId == new ExamCenterId(centerId));
        if (filter.InstructorId is { } instructorId)
            attemptQuery = attemptQuery.Where(x => x.InstructorId == new UserId(instructorId));

        List<AttemptRow> attempts = await attemptQuery
            .Select(x => new AttemptRow(
                x.Id.Value,
                x.RegistrationId.Value,
                x.ScheduledStartUtc,
                x.ExamType,
                x.LicenseCategory,
                x.ExamCenterId.Value,
                x.InstructorId.HasValue ? x.InstructorId.Value.Value : null,
                x.AttemptNumber,
                x.Status,
                x.AttendanceStatus))
            .ToListAsync(cancellationToken);

        Guid[] attemptIds = attempts.Select(x => x.AttemptId).Distinct().ToArray();
        Guid[] registrationIds = attempts.Select(x => x.RegistrationId).Distinct().ToArray();

        Dictionary<Guid, Guid?> branchByRegistration;
        if (registrationIds.Length == 0)
        {
            branchByRegistration = [];
        }
        else
        {
            ExamRegistrationId[] typedRegistrationIds = registrationIds
                .Select(static id => new ExamRegistrationId(id))
                .ToArray();
            var operationalPlanQuery = WhereStrongIdIn(
                dbContext.ExamOperationalPlans
                    .AsNoTracking()
                    .Where(x => x.OrganizationId == organizationId),
                x => x.RegistrationId,
                typedRegistrationIds);

            branchByRegistration = await operationalPlanQuery
                .Select(x => new
                {
                    RegistrationId = x.RegistrationId.Value,
                    BranchId = x.DepartureBranchId.HasValue ? (Guid?)x.DepartureBranchId.Value.Value : null
                })
                .ToDictionaryAsync(x => x.RegistrationId, x => x.BranchId, cancellationToken);
        }

        List<ResultRow> finalizedResults;
        if (attemptIds.Length == 0)
        {
            finalizedResults = [];
        }
        else
        {
            ExamAttemptId[] typedAttemptIds = attemptIds.Select(static id => new ExamAttemptId(id)).ToArray();
            var resultQuery = WhereStrongIdIn(
                dbContext.ExamResults
                    .AsNoTracking()
                    .Where(x => x.OrganizationId == organizationId && x.Status == ExamResultStatus.Finalized),
                x => x.AttemptId,
                typedAttemptIds);

            finalizedResults = await resultQuery
                .Select(x => new ResultRow(x.AttemptId.Value, x.Outcome, x.AttemptNumber, x.FailureReasonCode))
                .ToListAsync(cancellationToken);
        }

        int scheduled = attempts.Count;
        int absences = attempts.Count(x => x.Status == ExamAttemptStatus.CandidateAbsent || x.AttendanceStatus == ExamAttendanceStatus.Absent);
        int excusedAbsences = attempts.Count(x => x.AttendanceStatus == ExamAttendanceStatus.ExcusedAbsent);
        int cancelledOrPostponed = attempts.Count(x => x.Status is ExamAttemptStatus.Cancelled or ExamAttemptStatus.Postponed);
        int passed = finalizedResults.Count(x => x.Outcome == ExamResultOutcome.Passed);
        int failed = finalizedResults.Count(x => x.Outcome == ExamResultOutcome.Failed);
        int presented = passed + failed;
        int firstAttemptPresented = finalizedResults.Count(x => x.AttemptNumber == 1 && x.Outcome is ExamResultOutcome.Passed or ExamResultOutcome.Failed);
        int firstAttemptPassed = finalizedResults.Count(x => x.AttemptNumber == 1 && x.Outcome == ExamResultOutcome.Passed);

        int remediationRequired = await dbContext.ExamRemediationRequests.AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId
                && x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc <= toUtc
                && x.Status != ExamRemediationRequestStatus.Superseded, cancellationToken);
        int remediationValidated = await dbContext.ExamRemediationRequests.AsNoTracking()
            .CountAsync(x => x.OrganizationId == organizationId
                && x.CreatedAtUtc >= fromUtc && x.CreatedAtUtc <= toUtc
                && x.Status == ExamRemediationRequestStatus.ValidatedForRePresentation, cancellationToken);

        bool overallSmallSample = presented < _options.SmallSampleThreshold;
        var kpis = new ExamAnalyticsKpis(
            scheduled,
            presented,
            finalizedResults.Count,
            passed,
            failed,
            absences,
            excusedAbsences,
            cancelledOrPostponed,
            Percent(passed, presented),
            Percent(firstAttemptPassed, firstAttemptPresented),
            Percent(absences + excusedAbsences, scheduled),
            finalizedResults.Count == 0 ? null : decimal.Round(finalizedResults.Average(x => (decimal)x.AttemptNumber), 2),
            remediationRequired,
            remediationValidated,
            overallSmallSample,
            overallSmallSample ? "exams.analytics.sample.small" : null);

        Dictionary<Guid, ResultRow> resultByAttempt = finalizedResults.ToDictionary(x => x.AttemptId);

        IReadOnlyList<ExamAnalyticsSeriesPoint> monthly = attempts
            .GroupBy(x => x.ScheduledStartUtc.ToString("yyyy-MM"))
            .OrderBy(x => x.Key)
            .Select(g => BuildSeries(g.Key, g, resultByAttempt, "month"))
            .ToArray();

        IReadOnlyList<ExamAnalyticsSeriesPoint> byBranch = attempts
            .GroupBy(x => branchByRegistration.GetValueOrDefault(x.RegistrationId)?.ToString("D") ?? "unassigned")
            .OrderByDescending(g => g.Count())
            .Select(g => BuildSeries(g.Key, g, resultByAttempt, "branch"))
            .ToArray();

        IReadOnlyList<ExamAnalyticsSeriesPoint> byLicense = attempts
            .GroupBy(x => string.IsNullOrWhiteSpace(x.LicenseCategory) ? "unknown" : x.LicenseCategory)
            .OrderByDescending(g => g.Count())
            .Select(g => BuildSeries(g.Key, g, resultByAttempt, "licenseCategory"))
            .ToArray();

        IReadOnlyList<ExamAnalyticsSeriesPoint> byType = attempts
            .GroupBy(x => string.IsNullOrWhiteSpace(x.ExamType) ? "unknown" : x.ExamType)
            .OrderByDescending(g => g.Count())
            .Select(g => BuildSeries(g.Key, g, resultByAttempt, "examType"))
            .ToArray();

        IReadOnlyList<ExamAnalyticsSeriesPoint> byCenter = attempts
            .GroupBy(x => x.ExamCenterId.ToString("D"))
            .OrderByDescending(g => g.Count())
            .Select(g => BuildSeries(g.Key, g, resultByAttempt, "center"))
            .ToArray();

        IReadOnlyList<ExamAnalyticsSeriesPoint> byInstructor = attempts
            .GroupBy(x => x.InstructorId?.ToString("D") ?? "unassigned")
            .OrderByDescending(g => g.Count())
            .Select(g => BuildSeries(g.Key, g, resultByAttempt, "instructor"))
            .ToArray();

        IReadOnlyList<ExamAttemptDistributionPoint> byAttempt = finalizedResults
            .GroupBy(x => x.AttemptNumber)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                int groupPassed = g.Count(x => x.Outcome == ExamResultOutcome.Passed);
                int groupFailed = g.Count(x => x.Outcome == ExamResultOutcome.Failed);
                int denominator = groupPassed + groupFailed;
                bool small = denominator < _options.SmallSampleThreshold;
                return new ExamAttemptDistributionPoint(
                    g.Key,
                    g.Count(),
                    groupPassed,
                    groupFailed,
                    Percent(groupPassed, denominator),
                    small,
                    small ? "exams.analytics.attempt.smallSample" : null);
            })
            .ToArray();

        IReadOnlyList<ExamFailureReasonPoint> failureReasons = failed == 0
            ? []
            : finalizedResults
                .Where(x => x.Outcome == ExamResultOutcome.Failed && !string.IsNullOrWhiteSpace(x.FailureReasonCode))
                .GroupBy(x => x.FailureReasonCode!.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => new ExamFailureReasonPoint(g.Key, g.Count(), decimal.Round(g.Count() * 100m / failed, 2)))
                .ToArray();

        List<ExamAnalyticsAlert> alerts = [];
        AddTrendAlert(monthly, alerts);
        AddDimensionAlerts(byBranch, "branch", alerts);
        AddDimensionAlerts(byCenter, "center", alerts);
        AddDimensionAlerts(byInstructor, "instructor", alerts);
        AddFailureReasonAlert(failureReasons, failed, alerts);

        List<string> warnings = [];
        if (overallSmallSample)
            warnings.Add("exams.analytics.context.smallOverallSample");
        if (byBranch.Any(x => x.Key == "unassigned"))
            warnings.Add("exams.analytics.context.someAttemptsWithoutDepartureBranch");

        return new ExamAnalyticsResponse(
            fromUtc,
            toUtc,
            kpis,
            monthly,
            byBranch,
            byLicense,
            byType,
            byCenter,
            byInstructor,
            byAttempt,
            failureReasons,
            alerts,
            warnings);
    }

    private ExamAnalyticsSeriesPoint BuildSeries(
        string key,
        IEnumerable<AttemptRow> attempts,
        IReadOnlyDictionary<Guid, ResultRow> resultByAttempt,
        string dimension)
    {
        AttemptRow[] rows = attempts.ToArray();
        ResultRow[] results = rows
            .Select(x => resultByAttempt.GetValueOrDefault(x.AttemptId))
            .Where(x => x is not null)
            .Cast<ResultRow>()
            .ToArray();
        int passed = results.Count(x => x.Outcome == ExamResultOutcome.Passed);
        int failed = results.Count(x => x.Outcome == ExamResultOutcome.Failed);
        int presented = passed + failed;
        bool small = presented < _options.SmallSampleThreshold;
        return new ExamAnalyticsSeriesPoint(
            key,
            rows.Length,
            presented,
            passed,
            failed,
            Percent(passed, presented),
            small,
            small ? $"exams.analytics.{dimension}.smallSample" : null);
    }

    private void AddTrendAlert(IReadOnlyList<ExamAnalyticsSeriesPoint> trend, List<ExamAnalyticsAlert> alerts)
    {
        ExamAnalyticsSeriesPoint[] comparable = trend
            .Where(x => !x.IsSmallSample && x.PassRatePercent.HasValue)
            .ToArray();
        if (comparable.Length < 2)
            return;

        ExamAnalyticsSeriesPoint current = comparable[^1];
        ExamAnalyticsSeriesPoint previous = comparable[^2];
        decimal drop = previous.PassRatePercent!.Value - current.PassRatePercent!.Value;
        if (drop < _options.PassRateDropAlertPoints)
            return;

        alerts.Add(new ExamAnalyticsAlert(
            "Exams.Analytics.PassRateDrop",
            "exams.analytics.alerts.passRateDrop",
            "Warning",
            "month",
            current.Key,
            current.Presented,
            $"Pass rate decreased by {drop:0.0} points versus the previous comparable month.",
            "/api/exams/analytics/results"));
    }

    private void AddDimensionAlerts(
        IReadOnlyList<ExamAnalyticsSeriesPoint> items,
        string dimension,
        List<ExamAnalyticsAlert> alerts)
    {
        ExamAnalyticsSeriesPoint[] comparable = items
            .Where(x => !x.IsSmallSample && x.PassRatePercent.HasValue && x.Key != "unassigned")
            .ToArray();
        if (comparable.Length < 2)
            return;

        decimal average = comparable.Average(x => x.PassRatePercent!.Value);
        foreach (ExamAnalyticsSeriesPoint item in comparable.Where(x => average - x.PassRatePercent!.Value >= _options.ContextualUnderperformancePoints))
        {
            alerts.Add(new ExamAnalyticsAlert(
                "Exams.Analytics.ContextualUnderperformance",
                "exams.analytics.alerts.contextualUnderperformance",
                "Info",
                dimension,
                item.Key,
                item.Presented,
                $"Observed pass rate is {average - item.PassRatePercent!.Value:0.0} points below the comparable sample average. This is a signal, not a causal conclusion.",
                "/api/exams/analytics/results"));
        }
    }

    private void AddFailureReasonAlert(
        IReadOnlyList<ExamFailureReasonPoint> reasons,
        int failed,
        List<ExamAnalyticsAlert> alerts)
    {
        ExamFailureReasonPoint? top = reasons.FirstOrDefault();
        if (failed < _options.SmallSampleThreshold || top is null || top.PercentageOfFailures < _options.RecurrentFailureReasonPercent)
            return;

        alerts.Add(new ExamAnalyticsAlert(
            "Exams.Analytics.RecurrentFailureReason",
            "exams.analytics.alerts.recurrentFailureReason",
            "Info",
            "failureReason",
            top.ReasonCode,
            failed,
            $"Failure reason '{top.ReasonCode}' represents {top.PercentageOfFailures:0.0}% of coded failures in the selected period.",
            "/api/exams/analytics/results"));
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

    private static decimal? Percent(int numerator, int denominator)
        => denominator <= 0 ? null : decimal.Round(numerator * 100m / denominator, 2);
}
