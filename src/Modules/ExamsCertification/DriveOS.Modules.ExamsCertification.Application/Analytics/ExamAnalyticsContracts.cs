using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Analytics;

/// <summary>
/// Read-side filters for BC-11 result analytics. Dates are interpreted against the scheduled exam instant in UTC.
/// BranchId represents the operational departure branch captured by ExamOperationalPlan, not the student's administrative home branch.
/// </summary>
public sealed record ExamAnalyticsFilter(
    DateTimeOffset? FromUtc = null,
    DateTimeOffset? ToUtc = null,
    string? ExamType = null,
    string? LicenseCategory = null,
    Guid? ExamCenterId = null,
    Guid? InstructorId = null,
    Guid? BranchId = null);

public sealed record ExamAnalyticsKpis(
    int ScheduledAttempts,
    int PresentedAttempts,
    int FinalizedResults,
    int Passed,
    int Failed,
    int CandidateAbsences,
    int ExcusedAbsences,
    int CancelledOrPostponed,
    decimal? PassRatePercent,
    decimal? FirstAttemptPassRatePercent,
    decimal? AbsenceRatePercent,
    decimal? AverageAttemptNumber,
    int RemediationRequired,
    int RemediationValidatedForRePresentation,
    bool IsSmallSample,
    string? ContextNote);

public sealed record ExamAnalyticsSeriesPoint(
    string Key,
    int SampleSize,
    int Presented,
    int Passed,
    int Failed,
    decimal? PassRatePercent,
    bool IsSmallSample,
    string? ContextNote);

public sealed record ExamAttemptDistributionPoint(
    int AttemptNumber,
    int FinalizedResults,
    int Passed,
    int Failed,
    decimal? PassRatePercent,
    bool IsSmallSample,
    string? ContextNote);

public sealed record ExamFailureReasonPoint(
    string ReasonCode,
    int Count,
    decimal PercentageOfFailures);

public sealed record ExamAnalyticsAlert(
    string Code,
    string MessageKey,
    string Severity,
    string Dimension,
    string? DimensionKey,
    int SampleSize,
    string Explanation,
    string SourceRoute);

public sealed record ExamAnalyticsResponse(
    DateTimeOffset FromUtc,
    DateTimeOffset ToUtc,
    ExamAnalyticsKpis Kpis,
    IReadOnlyList<ExamAnalyticsSeriesPoint> MonthlyTrend,
    IReadOnlyList<ExamAnalyticsSeriesPoint> ByBranch,
    IReadOnlyList<ExamAnalyticsSeriesPoint> ByLicenseCategory,
    IReadOnlyList<ExamAnalyticsSeriesPoint> ByExamType,
    IReadOnlyList<ExamAnalyticsSeriesPoint> ByExamCenter,
    IReadOnlyList<ExamAnalyticsSeriesPoint> ByInstructor,
    IReadOnlyList<ExamAttemptDistributionPoint> ByAttemptNumber,
    IReadOnlyList<ExamFailureReasonPoint> FailureReasons,
    IReadOnlyList<ExamAnalyticsAlert> Alerts,
    IReadOnlyList<string> ContextWarnings);

public interface IExamAnalyticsReadService
{
    Task<ExamAnalyticsResponse> GetAsync(
        OrganizationId organizationId,
        ExamAnalyticsFilter filter,
        CancellationToken cancellationToken = default);
}

public sealed record GetExamAnalyticsQuery(OrganizationId OrganizationId, ExamAnalyticsFilter Filter)
    : IQuery<ExamAnalyticsResponse>;
