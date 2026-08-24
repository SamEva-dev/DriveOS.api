using DriveOS.Modules.Workforce.Application.ProfessionalEligibility;
using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Workforce.Infrastructure.Read;

internal sealed class WorkforceProfessionalEligibilityReadService(WorkforceDbContext db)
    : IWorkforceProfessionalEligibilityReadService
{
    public async Task<ProfessionalEligibilityResult> CheckAsync(
        OrganizationId organizationId,
        UserId userId,
        ProfessionalRestrictionActivity activity,
        DateOnly date,
        string? countryCode = null,
        string? licenseCategoryCode = null,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default)
    {
        Employee? employee = await db.Employees
            .AsNoTracking()
            .Include(x => x.BranchAssignments)
            .Include(x => x.InstructorAuthorizations)
            .Where(x => x.EmployerOrganizationId == organizationId && x.UserId == userId)
            .Where(x => x.Status != EmploymentStatus.Ended)
            .OrderByDescending(x => x.EmploymentStartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
            return new(false, "workforce.employee.not-current", null);

        if (date < employee.EmploymentStartDate ||
            (employee.EmploymentEndDate.HasValue && date > employee.EmploymentEndDate.Value))
            return new(false, "workforce.employee.outside-employment-period", null);

        if (employee.Status == EmploymentStatus.Suspended)
            return new(false, "workforce.employee.suspended", null);

        if (employee.Status is EmploymentStatus.Draft or EmploymentStatus.Onboarding or EmploymentStatus.Ending)
            return new(false, $"workforce.employee.status.{employee.Status.ToString().ToLowerInvariant()}", null);

        if (branchId.HasValue)
        {
            bool branchCovered = employee.BranchAssignments.Any(x =>
                x.BranchId == branchId.Value &&
                x.Status != EmployeeBranchAssignmentStatus.Cancelled &&
                x.StartDate <= date &&
                (!x.EndDate.HasValue || x.EndDate.Value >= date));

            if (!branchCovered)
                return new(false, "workforce.employee.branch-not-assigned", null);
        }

        string? normalizedCountry = Normalize(countryCode);
        string? normalizedCategory = Normalize(licenseCategoryCode);

        if (activity == ProfessionalRestrictionActivity.Teaching && normalizedCategory is not null)
        {
            bool hasVerifiedAuthorization = employee.InstructorAuthorizations.Any(x =>
                x.LicenseCategoryCode == normalizedCategory &&
                (normalizedCountry is null || x.CountryCode == normalizedCountry) &&
                x.IsVerifiedAt(date));

            if (!hasVerifiedAuthorization)
                return new(false, "workforce.instructor-authorization.missing-or-invalid", null);
        }

        List<ProfessionalRestriction> matches = await db.ProfessionalRestrictions
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.EmployeeId == employee.Id &&
                x.Status == ProfessionalRestrictionStatus.Active &&
                x.StartDate <= date &&
                (!x.EndDate.HasValue || x.EndDate.Value >= date) &&
                (x.Activity == ProfessionalRestrictionActivity.AllProfessionalDuties || x.Activity == activity))
            .OrderByDescending(x => x.Activity == ProfessionalRestrictionActivity.AllProfessionalDuties)
            .ToListAsync(cancellationToken);

        foreach (ProfessionalRestriction restriction in matches)
        {
            if (restriction.CountryCode is not null &&
                normalizedCountry is not null &&
                restriction.CountryCode != normalizedCountry)
                continue;

            if (restriction.LicenseCategoryCode is not null &&
                (normalizedCategory is null || restriction.LicenseCategoryCode != normalizedCategory))
                continue;

            if (restriction.BranchId.HasValue && restriction.BranchId != branchId)
                continue;

            return new(
                false,
                $"workforce.professional-restriction.{restriction.Activity.ToString().ToLowerInvariant()}",
                restriction.Id.Value);
        }

        return new(true, null, null);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
