using DriveOS.Modules.Workforce.Application.Dashboard;
using DriveOS.Modules.Workforce.Domain.EmployeeDocuments;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.Modules.Workforce.Domain.EquipmentAssignments;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.Modules.Workforce.Domain.PerformanceReviews;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Workforce.Infrastructure.Read;

internal sealed class WorkforceDashboardReadService(WorkforceDbContext dbContext)
    : IWorkforceDashboardReadService
{
    public async Task<WorkforceDashboardResponse> GetAsync(
        OrganizationId organizationId,
        DateOnly today,
        int alertWindowDays,
        CancellationToken cancellationToken = default)
    {
        var horizon = today.AddDays(alertWindowDays);
        var employees = dbContext.Employees.AsNoTracking()
            .Where(x => x.EmployerOrganizationId == organizationId);

        var currentEmployees = employees.Where(x => x.Status != EmploymentStatus.Ended);
        var currentEmployeeIds = currentEmployees.Select(x => x.Id);

        var headcount = new WorkforceHeadcountKpis(
            Current: await currentEmployees.CountAsync(cancellationToken),
            Active: await employees.CountAsync(x => x.Status == EmploymentStatus.Active, cancellationToken),
            Onboarding: await employees.CountAsync(x => x.Status == EmploymentStatus.Onboarding, cancellationToken),
            Suspended: await employees.CountAsync(x => x.Status == EmploymentStatus.Suspended, cancellationToken),
            OnLeave: await employees.CountAsync(x => x.Status == EmploymentStatus.OnLeave, cancellationToken),
            Ending: await employees.CountAsync(x => x.Status == EmploymentStatus.Ending, cancellationToken));

        var leaveQuery = dbContext.LeaveRequests.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId);
        var leave = new WorkforceLeaveKpis(
            PendingApproval: await leaveQuery.CountAsync(x => x.Status == LeaveRequestStatus.Submitted, cancellationToken),
            ActiveToday: await leaveQuery.CountAsync(x => x.Status == LeaveRequestStatus.Approved && x.StartDate <= today && x.EndDate >= today, cancellationToken),
            Upcoming: await leaveQuery.CountAsync(x => x.Status == LeaveRequestStatus.Approved && x.StartDate > today && x.StartDate <= horizon, cancellationToken));

        var contractPendingSignature = await employees
            .SelectMany(x => x.EmploymentContracts)
            .CountAsync(x => x.Status == EmploymentContractStatus.PendingSignature, cancellationToken);
        var contractExpiringSoon = await employees
            .SelectMany(x => x.EmploymentContracts)
            .CountAsync(x => x.EndDate.HasValue && x.EndDate.Value >= today && x.EndDate.Value <= horizon &&
                (x.Status == EmploymentContractStatus.Signed || x.Status == EmploymentContractStatus.Active || x.Status == EmploymentContractStatus.Suspended || x.Status == EmploymentContractStatus.Ending), cancellationToken);
        var contracts = new WorkforceContractKpis(
            PendingSignature: contractPendingSignature,
            ExpiringSoon: contractExpiringSoon,
            Ending: await employees.SelectMany(x => x.EmploymentContracts).CountAsync(x => x.Status == EmploymentContractStatus.Ending, cancellationToken));

        var authorizations = currentEmployees.SelectMany(x => x.InstructorAuthorizations)
            .Where(x => x.Status == EmployeeQualificationStatus.Verified && x.ExpiresOn.HasValue);
        var documents = dbContext.EmployeeDocuments.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                (x.Status == EmployeeDocumentStatus.Registered || x.Status == EmployeeDocumentStatus.Verified) &&
                x.ExpiresOn.HasValue);

        var compliance = new WorkforceComplianceKpis(
            InstructorAuthorizationsExpired: await authorizations.CountAsync(x => x.ExpiresOn!.Value < today, cancellationToken),
            InstructorAuthorizationsExpiringSoon: await authorizations.CountAsync(x => x.ExpiresOn!.Value >= today && x.ExpiresOn.Value <= horizon, cancellationToken),
            EmployeeDocumentsExpired: await documents.CountAsync(x => x.ExpiresOn!.Value < today, cancellationToken),
            EmployeeDocumentsExpiringSoon: await documents.CountAsync(x => x.ExpiresOn!.Value >= today && x.ExpiresOn.Value <= horizon, cancellationToken));

        var timesheetQuery = dbContext.Timesheets.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        var timesheets = new WorkforceTimesheetKpis(
            Submitted: await timesheetQuery.CountAsync(x => x.Status == TimesheetStatus.Submitted, cancellationToken),
            UnderReview: await timesheetQuery.CountAsync(x => x.Status == TimesheetStatus.UnderReview, cancellationToken),
            ApprovedAwaitingLock: await timesheetQuery.CountAsync(x => x.Status == TimesheetStatus.Approved, cancellationToken));

        var equipmentQuery = dbContext.EquipmentAssignments.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        var equipment = new WorkforceEquipmentKpis(
            Planned: await equipmentQuery.CountAsync(x => x.Status == EquipmentAssignmentStatus.Planned, cancellationToken),
            Active: await equipmentQuery.CountAsync(x => x.Status == EquipmentAssignmentStatus.Active, cancellationToken),
            ReturnOverdue: await equipmentQuery.CountAsync(x => x.Status == EquipmentAssignmentStatus.Active && x.PlannedEndDate.HasValue && x.PlannedEndDate.Value < today, cancellationToken),
            HeldByEndedEmployees: await equipmentQuery.CountAsync(x => x.Status == EquipmentAssignmentStatus.Active &&
                employees.Any(e => e.Id == x.EmployeeId && e.Status == EmploymentStatus.Ended), cancellationToken));

        var reviewQuery = dbContext.PerformanceReviews.AsNoTracking().Where(x => x.OrganizationId == organizationId);
        var reviews = new WorkforceReviewKpis(
            InProgress: await reviewQuery.CountAsync(x => x.Status == PerformanceReviewStatus.InProgress, cancellationToken),
            AwaitingAcknowledgement: await reviewQuery.CountAsync(x => x.Status == PerformanceReviewStatus.Submitted, cancellationToken));

        var alerts = new List<WorkforceDashboardAlert>();

        var endingEmployees = await employees
            .Where(x => x.Status == EmploymentStatus.Ending && x.EmploymentEndDate.HasValue && x.EmploymentEndDate.Value <= horizon)
            .OrderBy(x => x.EmploymentEndDate)
            .Select(x => new { x.Id, x.EmploymentEndDate })
            .Take(20)
            .ToListAsync(cancellationToken);
        alerts.AddRange(endingEmployees.Select(x => Alert(
            "employment-ending", "warning", x.Id.Value, x.Id.Value, x.EmploymentEndDate,
            "workforce.dashboard.alerts.employmentEnding", Days(today, x.EmploymentEndDate))));

        var expiringContracts = await employees
            .SelectMany(e => e.EmploymentContracts.Select(c => new { EmployeeId = e.Id, Contract = c }))
            .Where(x => x.Contract.EndDate.HasValue && x.Contract.EndDate.Value >= today && x.Contract.EndDate.Value <= horizon &&
                (x.Contract.Status == EmploymentContractStatus.Signed || x.Contract.Status == EmploymentContractStatus.Active || x.Contract.Status == EmploymentContractStatus.Suspended || x.Contract.Status == EmploymentContractStatus.Ending))
            .OrderBy(x => x.Contract.EndDate)
            .Select(x => new { x.EmployeeId, ContractId = x.Contract.Id, x.Contract.EndDate })
            .Take(20)
            .ToListAsync(cancellationToken);
        alerts.AddRange(expiringContracts.Select(x => Alert(
            "contract-expiring", "warning", x.EmployeeId.Value, x.ContractId.Value, x.EndDate,
            "workforce.dashboard.alerts.contractExpiring", Days(today, x.EndDate))));

        var expiringAuthorizations = await currentEmployees
            .SelectMany(e => e.InstructorAuthorizations.Select(a => new { EmployeeId = e.Id, Authorization = a }))
            .Where(x => x.Authorization.Status == EmployeeQualificationStatus.Verified && x.Authorization.ExpiresOn.HasValue && x.Authorization.ExpiresOn.Value <= horizon)
            .OrderBy(x => x.Authorization.ExpiresOn)
            .Select(x => new { x.EmployeeId, AuthorizationId = x.Authorization.Id, x.Authorization.ExpiresOn, x.Authorization.LicenseCategoryCode })
            .Take(20)
            .ToListAsync(cancellationToken);
        alerts.AddRange(expiringAuthorizations.Select(x => Alert(
            "instructor-authorization-expiry", x.ExpiresOn!.Value < today ? "critical" : "warning", x.EmployeeId.Value, x.AuthorizationId.Value, x.ExpiresOn,
            x.ExpiresOn.Value < today ? "workforce.dashboard.alerts.instructorAuthorizationExpired" : "workforce.dashboard.alerts.instructorAuthorizationExpiring",
            new Dictionary<string, string?> { ["days"] = (x.ExpiresOn.Value.DayNumber - today.DayNumber).ToString(), ["category"] = x.LicenseCategoryCode })));

        var expiringDocuments = await documents
            .Where(x => x.ExpiresOn!.Value <= horizon)
            .OrderBy(x => x.ExpiresOn)
            .Select(x => new { x.EmployeeId, x.Id, x.ExpiresOn, x.DocumentTypeCode })
            .Take(20)
            .ToListAsync(cancellationToken);
        alerts.AddRange(expiringDocuments.Select(x => Alert(
            "employee-document-expiry", x.ExpiresOn!.Value < today ? "critical" : "warning", x.EmployeeId.Value, x.Id.Value, x.ExpiresOn,
            x.ExpiresOn.Value < today ? "workforce.dashboard.alerts.employeeDocumentExpired" : "workforce.dashboard.alerts.employeeDocumentExpiring",
            new Dictionary<string, string?> { ["days"] = (x.ExpiresOn.Value.DayNumber - today.DayNumber).ToString(), ["documentType"] = x.DocumentTypeCode })));

        var overdueEquipment = await equipmentQuery
            .Where(x => x.Status == EquipmentAssignmentStatus.Active && x.PlannedEndDate.HasValue && x.PlannedEndDate.Value < today)
            .OrderBy(x => x.PlannedEndDate)
            .Select(x => new { x.EmployeeId, x.Id, x.PlannedEndDate, x.ResourceType })
            .Take(20)
            .ToListAsync(cancellationToken);
        alerts.AddRange(overdueEquipment.Select(x => Alert(
            "equipment-return-overdue", "critical", x.EmployeeId.Value, x.Id.Value, x.PlannedEndDate,
            "workforce.dashboard.alerts.equipmentReturnOverdue",
            new Dictionary<string, string?> { ["days"] = (today.DayNumber - x.PlannedEndDate!.Value.DayNumber).ToString(), ["resourceType"] = x.ResourceType.ToString() })));

        alerts = alerts
            .OrderBy(x => x.Severity == "critical" ? 0 : 1)
            .ThenBy(x => x.DueDate)
            .Take(50)
            .ToList();

        return new WorkforceDashboardResponse(
            today,
            alertWindowDays,
            headcount,
            leave,
            contracts,
            compliance,
            timesheets,
            equipment,
            reviews,
            alerts);
    }

    private static WorkforceDashboardAlert Alert(
        string kind,
        string severity,
        Guid? employeeId,
        Guid? referenceId,
        DateOnly? dueDate,
        string messageKey,
        IReadOnlyDictionary<string, string?> parameters)
        => new(kind, severity, employeeId, referenceId, dueDate, messageKey, parameters);

    private static IReadOnlyDictionary<string, string?> Days(DateOnly today, DateOnly? dueDate)
        => new Dictionary<string, string?>
        {
            ["days"] = dueDate.HasValue ? (dueDate.Value.DayNumber - today.DayNumber).ToString() : null
        };
}
