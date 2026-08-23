using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Workforce.Application.Dashboard;

public sealed record GetWorkforceDashboardQuery(
    OrganizationId OrganizationId,
    int AlertWindowDays = 30) : IQuery<WorkforceDashboardResponse>;

public sealed record WorkforceHeadcountKpis(
    int Current,
    int Active,
    int Onboarding,
    int Suspended,
    int OnLeave,
    int Ending);

public sealed record WorkforceLeaveKpis(
    int PendingApproval,
    int ActiveToday,
    int Upcoming);

public sealed record WorkforceContractKpis(
    int PendingSignature,
    int ExpiringSoon,
    int Ending);

public sealed record WorkforceComplianceKpis(
    int InstructorAuthorizationsExpired,
    int InstructorAuthorizationsExpiringSoon,
    int EmployeeDocumentsExpired,
    int EmployeeDocumentsExpiringSoon);

public sealed record WorkforceTimesheetKpis(
    int Submitted,
    int UnderReview,
    int ApprovedAwaitingLock);

public sealed record WorkforceEquipmentKpis(
    int Planned,
    int Active,
    int ReturnOverdue,
    int HeldByEndedEmployees);

public sealed record WorkforceReviewKpis(
    int InProgress,
    int AwaitingAcknowledgement);

public sealed record WorkforceDashboardAlert(
    string Kind,
    string Severity,
    Guid? EmployeeId,
    Guid? ReferenceId,
    DateOnly? DueDate,
    string MessageKey,
    IReadOnlyDictionary<string, string?> Parameters);

public sealed record WorkforceDashboardResponse(
    DateOnly AsOfDate,
    int AlertWindowDays,
    WorkforceHeadcountKpis Headcount,
    WorkforceLeaveKpis Leave,
    WorkforceContractKpis Contracts,
    WorkforceComplianceKpis Compliance,
    WorkforceTimesheetKpis Timesheets,
    WorkforceEquipmentKpis Equipment,
    WorkforceReviewKpis Reviews,
    IReadOnlyList<WorkforceDashboardAlert> Alerts);

public interface IWorkforceDashboardReadService
{
    Task<WorkforceDashboardResponse> GetAsync(
        OrganizationId organizationId,
        DateOnly today,
        int alertWindowDays,
        CancellationToken cancellationToken = default);
}
