using DriveOS.Modules.Workforce.Application.ProfessionalEligibility;using DriveOS.Modules.Workforce.Domain.Employees;using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;using DriveOS.Modules.Workforce.Infrastructure.Persistence;using DriveOS.SharedKernel.Identifiers;using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Read;
internal sealed class WorkforceProfessionalEligibilityReadService(WorkforceDbContext db):IWorkforceProfessionalEligibilityReadService
{
 public async Task<ProfessionalEligibilityResult> CheckAsync(OrganizationId org,UserId userId,ProfessionalRestrictionActivity activity,DateOnly date,string? countryCode=null,string? licenseCategoryCode=null,BranchId? branchId=null,CancellationToken cancellationToken=default)
 {
  var employee=await db.Employees.AsNoTracking().Where(x=>x.EmployerOrganizationId==org&&x.UserId==userId&&x.Status!=EmploymentStatus.Ended).OrderByDescending(x=>x.EmploymentStartDate).Select(x=>new{x.Id,x.Status}).FirstOrDefaultAsync(cancellationToken);
  if(employee is null)return new(false,"workforce.employee.not-current",null);
  if(employee.Status==EmploymentStatus.Suspended)return new(false,"workforce.employee.suspended",null);
  if(employee.Status is EmploymentStatus.Draft or EmploymentStatus.Onboarding or EmploymentStatus.Ending)return new(false,$"workforce.employee.status.{employee.Status.ToString().ToLowerInvariant()}",null);
  string? cc=N(countryCode),cat=N(licenseCategoryCode);
  var matches=await db.ProfessionalRestrictions.AsNoTracking().Where(x=>x.OrganizationId==org&&x.EmployeeId==employee.Id&&x.Status==ProfessionalRestrictionStatus.Active&&x.StartDate<=date&&(!x.EndDate.HasValue||x.EndDate.Value>=date)&&(x.Activity==ProfessionalRestrictionActivity.AllProfessionalDuties||x.Activity==activity)).OrderByDescending(x=>x.Activity==ProfessionalRestrictionActivity.AllProfessionalDuties).ToListAsync(cancellationToken);
  foreach(var x in matches){if(x.CountryCode is not null&&x.CountryCode!=cc)continue;if(x.LicenseCategoryCode is not null&&(cat is null||x.LicenseCategoryCode!=cat))continue;if(x.BranchId.HasValue&&x.BranchId!=branchId)continue;return new(false,$"workforce.professional-restriction.{x.Activity.ToString().ToLowerInvariant()}",x.Id.Value);}
  return new(true,null,null);
 }
 private static string? N(string? x)=>string.IsNullOrWhiteSpace(x)?null:x.Trim().ToUpperInvariant();
}
