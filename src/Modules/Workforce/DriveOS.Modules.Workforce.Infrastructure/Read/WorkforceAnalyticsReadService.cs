using DriveOS.Modules.Workforce.Application.Analytics;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Domain.Qualifications;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.Modules.Workforce.Domain.WorkingTime;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Workforce.Infrastructure.Read;

internal sealed class WorkforceAnalyticsReadService(WorkforceDbContext dbContext)
    : IWorkforceAnalyticsReadService
{
    public async Task<WorkforceAnalyticsResponse> GetAsync(
        OrganizationId organizationId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var employees = await dbContext.Employees
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.JobPositionAssignments)
            .Include(x => x.InstructorAuthorizations)
            .Include(x => x.EmploymentContracts)
            .Where(x => x.EmployerOrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        var jobPositions = await dbContext.JobPositions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var leaveRequests = await dbContext.LeaveRequests
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.Status == LeaveRequestStatus.Approved &&
                x.StartDate <= to && x.EndDate >= from)
            .ToListAsync(cancellationToken);

        var timesheets = await dbContext.Timesheets
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Entries)
            .Where(x => x.OrganizationId == organizationId &&
                x.PeriodFrom <= to && x.PeriodTo >= from)
            .ToListAsync(cancellationToken);

        var workingTimePolicies = await dbContext.WorkingTimePolicies
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.Status == WorkingTimePolicyStatus.Active &&
                x.EffectiveFrom <= to &&
                (!x.EffectiveTo.HasValue || x.EffectiveTo.Value >= from))
            .ToListAsync(cancellationToken);

        var activeRestrictions = await dbContext.ProfessionalRestrictions
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId &&
                x.Status == ProfessionalRestrictionStatus.Active &&
                x.StartDate <= to &&
                (!x.EndDate.HasValue || x.EndDate.Value >= to))
            .CountAsync(cancellationToken);

        var headcountAtStart = employees.Count(x => IsEmployedAt(x, from));
        var headcountAtEnd = employees.Count(x => IsEmployedAt(x, to));
        var hires = employees.Count(x => x.EmploymentStartDate >= from && x.EmploymentStartDate <= to && !x.RehiredFromEmployeeId.HasValue);
        var rehires = employees.Count(x => x.EmploymentStartDate >= from && x.EmploymentStartDate <= to && x.RehiredFromEmployeeId.HasValue);
        var exits = employees.Count(x => x.Status == EmploymentStatus.Ended && x.EmploymentEndDate.HasValue && x.EmploymentEndDate.Value >= from && x.EmploymentEndDate.Value <= to);
        var averageHeadcount = (headcountAtStart + headcountAtEnd) / 2m;
        var turnover = Percent(exits, averageHeadcount);

        var currentAtEnd = employees.Where(x => IsEmployedAt(x, to)).ToList();
        var averageTenureDays = currentAtEnd.Count == 0
            ? 0m
            : Math.Round(currentAtEnd.Average(x => (decimal)(to.DayNumber - x.EmploymentStartDate.DayNumber + 1)), 2);

        var approvedLeaveDays = leaveRequests.Sum(x => LeaveDayEquivalents(x, from, to));
        var employedCalendarDays = employees.Sum(x => EmploymentCalendarDaysInPeriod(x, from, to));
        var absenceRate = Percent(approvedLeaveDays, employedCalendarDays);

        var validatedTimesheets = timesheets
            .Where(x => x.Status is TimesheetStatus.Approved or TimesheetStatus.Locked)
            .ToList();

        var validatedEntries = validatedTimesheets
            .SelectMany(x => x.Entries)
            .Where(x => x.Date >= from && x.Date <= to)
            .ToList();

        decimal Hours(TimesheetActivityType type) => validatedEntries
            .Where(x => x.ActivityType == type)
            .Sum(x => x.Hours);

        var contractualHours = workingTimePolicies.Sum(x => ContractualHoursInPeriod(x, from, to));
        var validatedHours = validatedEntries.Sum(x => x.Hours);

        var instructorJobPositionIds = jobPositions.Values
            .Where(x => x.ProfessionalFunction == ProfessionalFunction.Instructor)
            .Select(x => x.Id)
            .ToHashSet();

        var currentInstructorEmployees = currentAtEnd
            .Where(x => x.JobPositionAssignments.Any(a =>
                a.Status != EmployeeJobPositionAssignmentStatus.Cancelled &&
                instructorJobPositionIds.Contains(a.JobPositionId) &&
                a.StartDate <= to &&
                (!a.EndDate.HasValue || a.EndDate.Value >= to)))
            .ToList();

        var withVerifiedAuthorization = currentInstructorEmployees.Count(x =>
            x.InstructorAuthorizations.Any(a => a.IsVerifiedAt(to)));

        var expiredAuthorizations = currentInstructorEmployees.Sum(x =>
            x.InstructorAuthorizations.Count(a =>
                a.Status == EmployeeQualificationStatus.Verified &&
                a.ExpiresOn.HasValue &&
                a.ExpiresOn.Value < to));

        var allContracts = employees
            .SelectMany(e => e.EmploymentContracts.Select(c => new { Employee = e, Contract = c }))
            .ToList();

        var contractsStarted = allContracts.Count(x => x.Contract.StartDate >= from && x.Contract.StartDate <= to && x.Contract.Status != EmploymentContractStatus.Cancelled);
        var contractsEnded = allContracts.Count(x => x.Contract.EndDate.HasValue && x.Contract.EndDate.Value >= from && x.Contract.EndDate.Value <= to && x.Contract.Status is EmploymentContractStatus.Terminated or EmploymentContractStatus.Completed);
        var contractsAtEnd = allContracts.Where(x =>
            x.Contract.Status != EmploymentContractStatus.Cancelled &&
            x.Contract.StartDate <= to &&
            (!x.Contract.EndDate.HasValue || x.Contract.EndDate.Value >= to)).ToList();

        var timesheetTotal = timesheets.Count;
        var timesheetLocked = timesheets.Count(x => x.Status == TimesheetStatus.Locked);
        var timesheetApproved = timesheets.Count(x => x.Status == TimesheetStatus.Approved);
        var timesheetRejected = timesheets.Count(x => x.Status == TimesheetStatus.Rejected);
        var timesheetPending = timesheets.Count(x => x.Status is TimesheetStatus.Submitted or TimesheetStatus.UnderReview);

        var monthlyTrend = BuildMonthlyTrend(from, to, employees, leaveRequests, validatedEntries);
        var functionBreakdown = BuildFunctionBreakdown(to, currentAtEnd, jobPositions);

        return new WorkforceAnalyticsResponse(
            from,
            to,
            new WorkforceAnalyticsHeadcount(
                headcountAtStart,
                headcountAtEnd,
                hires,
                rehires,
                exits,
                turnover,
                averageTenureDays),
            new WorkforceAnalyticsAbsence(
                leaveRequests.Count,
                Round(approvedLeaveDays),
                leaveRequests.Select(x => x.EmployeeId).Distinct().Count(),
                absenceRate),
            new WorkforceAnalyticsWorkingTime(
                Round(contractualHours),
                Round(validatedHours),
                Round(Hours(TimesheetActivityType.Teaching)),
                Round(Hours(TimesheetActivityType.Exam)),
                Round(Hours(TimesheetActivityType.Administrative)),
                Round(Hours(TimesheetActivityType.Travel)),
                Round(Hours(TimesheetActivityType.Meeting)),
                Round(Hours(TimesheetActivityType.Training)),
                Round(Hours(TimesheetActivityType.Leave)),
                Round(Hours(TimesheetActivityType.Other)),
                Percent(validatedHours, contractualHours)),
            new WorkforceAnalyticsCompliance(
                currentInstructorEmployees.Count,
                withVerifiedAuthorization,
                Percent(withVerifiedAuthorization, currentInstructorEmployees.Count),
                expiredAuthorizations,
                activeRestrictions),
            new WorkforceAnalyticsContracts(
                contractsStarted,
                contractsEnded,
                contractsAtEnd.Count(x => x.Contract.Status is EmploymentContractStatus.Signed or EmploymentContractStatus.Active or EmploymentContractStatus.Suspended or EmploymentContractStatus.Ending),
                contractsAtEnd.Count(x => x.Contract.ContractType == EmploymentContractType.FixedTerm && x.Contract.Status is not EmploymentContractStatus.Cancelled),
                contractsAtEnd.Count(x => x.Contract.Status == EmploymentContractStatus.PendingSignature)),
            new WorkforceAnalyticsTimesheets(
                timesheetTotal,
                timesheetLocked,
                timesheetApproved,
                timesheetRejected,
                timesheetPending,
                Percent(timesheetLocked, timesheetTotal)),
            monthlyTrend,
            functionBreakdown,
            Definitions());
    }

    private static bool IsEmployedAt(Employee employee, DateOnly date)
        => employee.Status != EmploymentStatus.Draft &&
           employee.EmploymentStartDate <= date &&
           (!employee.EmploymentEndDate.HasValue || employee.EmploymentEndDate.Value >= date);

    private static decimal EmploymentCalendarDaysInPeriod(Employee employee, DateOnly from, DateOnly to)
    {
        if (employee.Status == EmploymentStatus.Draft)
            return 0m;

        var start = employee.EmploymentStartDate > from ? employee.EmploymentStartDate : from;
        var end = employee.EmploymentEndDate.HasValue && employee.EmploymentEndDate.Value < to
            ? employee.EmploymentEndDate.Value
            : to;

        return end < start ? 0m : end.DayNumber - start.DayNumber + 1;
    }

    private static decimal LeaveDayEquivalents(LeaveRequest request, DateOnly from, DateOnly to)
    {
        var start = request.StartDate > from ? request.StartDate : from;
        var end = request.EndDate < to ? request.EndDate : to;
        if (end < start)
            return 0m;

        var days = (decimal)(end.DayNumber - start.DayNumber + 1);

        if (start == end)
        {
            var originalSingleDay = request.StartDate == request.EndDate;
            if (originalSingleDay && request.StartPortion != LeaveDayPortion.FullDay)
                return 0.5m;

            var clippedAtStart = start == request.StartDate && request.StartPortion != LeaveDayPortion.FullDay;
            var clippedAtEnd = end == request.EndDate && request.EndPortion != LeaveDayPortion.FullDay;
            return clippedAtStart || clippedAtEnd ? 0.5m : 1m;
        }

        if (start == request.StartDate && request.StartPortion != LeaveDayPortion.FullDay)
            days -= 0.5m;
        if (end == request.EndDate && request.EndPortion != LeaveDayPortion.FullDay)
            days -= 0.5m;

        return days;
    }

    private static decimal ContractualHoursInPeriod(WorkingTimePolicy policy, DateOnly from, DateOnly to)
    {
        var start = policy.EffectiveFrom > from ? policy.EffectiveFrom : from;
        var end = policy.EffectiveTo.HasValue && policy.EffectiveTo.Value < to ? policy.EffectiveTo.Value : to;
        if (end < start)
            return 0m;

        var calendarDays = end.DayNumber - start.DayNumber + 1;
        return policy.ContractualWeeklyHours * calendarDays / 7m;
    }

    private static IReadOnlyList<WorkforceAnalyticsMonthlyPoint> BuildMonthlyTrend(
        DateOnly from,
        DateOnly to,
        IReadOnlyList<Employee> employees,
        IReadOnlyList<LeaveRequest> leaveRequests,
        IReadOnlyList<TimesheetEntry> validatedEntries)
    {
        var points = new List<WorkforceAnalyticsMonthlyPoint>();
        var cursor = new DateOnly(from.Year, from.Month, 1);

        while (cursor <= to)
        {
            var monthStart = cursor < from ? from : cursor;
            var monthEndCandidate = cursor.AddMonths(1).AddDays(-1);
            var monthEnd = monthEndCandidate > to ? to : monthEndCandidate;

            points.Add(new WorkforceAnalyticsMonthlyPoint(
                cursor.Year,
                cursor.Month,
                employees.Count(x => x.EmploymentStartDate >= monthStart && x.EmploymentStartDate <= monthEnd),
                employees.Count(x => x.Status == EmploymentStatus.Ended && x.EmploymentEndDate.HasValue && x.EmploymentEndDate.Value >= monthStart && x.EmploymentEndDate.Value <= monthEnd),
                Round(leaveRequests.Sum(x => LeaveDayEquivalents(x, monthStart, monthEnd))),
                Round(validatedEntries.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Hours))));

            cursor = cursor.AddMonths(1);
        }

        return points;
    }

    private static IReadOnlyList<WorkforceAnalyticsBreakdown> BuildFunctionBreakdown(
        DateOnly at,
        IReadOnlyList<Employee> employees,
        IReadOnlyDictionary<JobPositionId, JobPosition> jobPositions)
    {
        var rows = employees
            .SelectMany(e => e.JobPositionAssignments
                .Where(a =>
                    a.Status != EmployeeJobPositionAssignmentStatus.Cancelled &&
                    a.StartDate <= at &&
                    (!a.EndDate.HasValue || a.EndDate.Value >= at) &&
                    jobPositions.ContainsKey(a.JobPositionId))
                .Select(a => new { e.Id, Function = jobPositions[a.JobPositionId].ProfessionalFunction }))
            .GroupBy(x => x.Function)
            .Select(g => new WorkforceAnalyticsBreakdown(
                g.Key.ToString(),
                $"workforce.professionalFunctions.{g.Key}",
                g.Select(x => x.Id).Distinct().Count()))
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key)
            .ToList();

        return rows;
    }

    private static decimal Percent(decimal numerator, decimal denominator)
        => denominator <= 0m ? 0m : Math.Round(numerator / denominator * 100m, 2);

    private static decimal Round(decimal value) => Math.Round(value, 2);

    private static IReadOnlyDictionary<string, string> Definitions()
        => new Dictionary<string, string>
        {
            ["turnoverRatePercent"] = "workforce.analytics.definitions.turnoverRatePercent",
            ["averageTenureDaysAtEnd"] = "workforce.analytics.definitions.averageTenureDaysAtEnd",
            ["approvedCalendarDayEquivalents"] = "workforce.analytics.definitions.approvedCalendarDayEquivalents",
            ["absenceRatePercent"] = "workforce.analytics.definitions.absenceRatePercent",
            ["contractualHours"] = "workforce.analytics.definitions.contractualHours",
            ["validatedTimesheetHours"] = "workforce.analytics.definitions.validatedTimesheetHours",
            ["validatedToContractPercent"] = "workforce.analytics.definitions.validatedToContractPercent",
            ["teachingAuthorizationCoveragePercent"] = "workforce.analytics.definitions.teachingAuthorizationCoveragePercent",
            ["timesheetLockRatePercent"] = "workforce.analytics.definitions.timesheetLockRatePercent"
        };
}
