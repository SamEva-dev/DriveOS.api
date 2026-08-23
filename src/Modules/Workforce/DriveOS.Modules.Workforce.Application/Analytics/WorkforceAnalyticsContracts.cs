using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Workforce.Application.Analytics;

public sealed record GetWorkforceAnalyticsQuery(
    OrganizationId OrganizationId,
    DateOnly From,
    DateOnly To) : IQuery<WorkforceAnalyticsResponse>;

public sealed record WorkforceAnalyticsHeadcount(
    int HeadcountAtStart,
    int HeadcountAtEnd,
    int Hires,
    int Rehires,
    int Exits,
    decimal TurnoverRatePercent,
    decimal AverageTenureDaysAtEnd);

public sealed record WorkforceAnalyticsAbsence(
    int ApprovedRequests,
    decimal ApprovedCalendarDayEquivalents,
    int EmployeesWithApprovedLeave,
    decimal AbsenceRatePercent);

public sealed record WorkforceAnalyticsWorkingTime(
    decimal ContractualHours,
    decimal ValidatedTimesheetHours,
    decimal TeachingHours,
    decimal ExamHours,
    decimal AdministrativeHours,
    decimal TravelHours,
    decimal MeetingHours,
    decimal TrainingHours,
    decimal LeaveHours,
    decimal OtherHours,
    decimal ValidatedToContractPercent);

public sealed record WorkforceAnalyticsCompliance(
    int CurrentInstructorEmployees,
    int WithVerifiedCurrentTeachingAuthorization,
    decimal TeachingAuthorizationCoveragePercent,
    int ExpiredTeachingAuthorizations,
    int ActiveProfessionalRestrictions);

public sealed record WorkforceAnalyticsContracts(
    int Started,
    int Ended,
    int ActiveAtEnd,
    int FixedTermActiveAtEnd,
    int PendingSignatureAtEnd);

public sealed record WorkforceAnalyticsTimesheets(
    int Total,
    int Locked,
    int Approved,
    int Rejected,
    int PendingReview,
    decimal LockRatePercent);

public sealed record WorkforceAnalyticsMonthlyPoint(
    int Year,
    int Month,
    int Hires,
    int Exits,
    decimal ApprovedLeaveDayEquivalents,
    decimal ValidatedTimesheetHours);

public sealed record WorkforceAnalyticsBreakdown(
    string Key,
    string Label,
    decimal Value);

public sealed record WorkforceAnalyticsResponse(
    DateOnly From,
    DateOnly To,
    WorkforceAnalyticsHeadcount Headcount,
    WorkforceAnalyticsAbsence Absence,
    WorkforceAnalyticsWorkingTime WorkingTime,
    WorkforceAnalyticsCompliance Compliance,
    WorkforceAnalyticsContracts Contracts,
    WorkforceAnalyticsTimesheets Timesheets,
    IReadOnlyList<WorkforceAnalyticsMonthlyPoint> MonthlyTrend,
    IReadOnlyList<WorkforceAnalyticsBreakdown> CurrentHeadcountByProfessionalFunction,
    IReadOnlyDictionary<string, string> MetricDefinitions);

public interface IWorkforceAnalyticsReadService
{
    Task<WorkforceAnalyticsResponse> GetAsync(
        OrganizationId organizationId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}

public static class WorkforceAnalyticsErrors
{
    public static readonly Error InvalidPeriod = Error.Validation(
        "Workforce.Analytics.InvalidPeriod",
        "workforce.analytics.errors.invalidPeriod");

    public static readonly Error PeriodTooLarge = Error.Validation(
        "Workforce.Analytics.PeriodTooLarge",
        "workforce.analytics.errors.periodTooLarge");
}
