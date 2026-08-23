using DriveOS.Modules.Workforce.Application.Availability;
using DriveOS.Modules.Workforce.Application.ProfessionalEligibility;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Workforce.Infrastructure.Read;

internal sealed class WorkforceAvailabilityReadService(WorkforceDbContext db, IWorkforceProfessionalEligibilityReadService eligibility) : IWorkforceAvailabilityReadService
{
    public async Task<IReadOnlyCollection<WorkforceAbsenceSnapshot>> ListApprovedAbsencesAsync(
        OrganizationId organizationId,
        UserId userId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        EmployeeId? employeeId = await db.Employees.AsNoTracking()
            .Where(x => x.EmployerOrganizationId == organizationId && x.UserId == userId)
            .Where(x => x.Status != EmploymentStatus.Ended)
            .OrderByDescending(x => x.EmploymentStartDate)
            .Select(x => (EmployeeId?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (employeeId is null)
            return Array.Empty<WorkforceAbsenceSnapshot>();

        return await db.LeaveRequests.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.EmployeeId == employeeId.Value
                && x.Status == LeaveRequestStatus.Approved
                && x.StartDate <= to
                && x.EndDate >= from)
            .OrderBy(x => x.StartDate)
            .Select(x => new WorkforceAbsenceSnapshot(
                x.Id.Value, x.EmployeeId.Value, x.StartDate, x.EndDate,
                (int)x.StartPortion, (int)x.EndPortion, x.PolicyCode, x.Reason))
            .ToListAsync(cancellationToken);
    }
    public async Task<WorkforceEmploymentAvailabilitySnapshot> CheckTeachingAvailabilityAsync(OrganizationId organizationId, UserId userId, DateOnly date, BranchId? branchId = null, string? licenseCategoryCode = null, CancellationToken cancellationToken = default)
    {
        ProfessionalEligibilityResult result = await eligibility.CheckAsync(organizationId, userId, ProfessionalRestrictionActivity.Teaching, date, null, licenseCategoryCode, branchId, cancellationToken);
        return new WorkforceEmploymentAvailabilitySnapshot(result.IsEligible, result.ReasonCode, result.RestrictionId);
    }

}
