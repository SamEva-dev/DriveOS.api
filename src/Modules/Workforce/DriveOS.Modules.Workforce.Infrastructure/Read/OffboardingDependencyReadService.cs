using DriveOS.Modules.Workforce.Application.Offboarding;
using DriveOS.Modules.Workforce.Domain.BranchAssignments;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.EmploymentContracts;
using DriveOS.Modules.Workforce.Domain.EquipmentAssignments;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Read;
internal sealed class OffboardingDependencyReadService(WorkforceDbContext db):IOffboardingDependencyReadService
{
 public async Task<OffboardingDependencySnapshot> GetAsync(OrganizationId org,EmployeeId employeeId,DateOnly plannedEndDate,CancellationToken ct=default)
 {
   var e=await db.Employees.AsNoTracking().Include(x=>x.BranchAssignments).Include(x=>x.JobPositionAssignments).Include(x=>x.EmploymentContracts).SingleOrDefaultAsync(x=>x.EmployerOrganizationId==org&&x.Id==employeeId,ct);
   if(e is null)return new(0,0,0,0,0,0);
   int branches=e.BranchAssignments.Count(x=>x.Status!=EmployeeBranchAssignmentStatus.Cancelled && (!x.EndDate.HasValue||x.EndDate.Value>plannedEndDate));
   int positions=e.JobPositionAssignments.Count(x=>x.Status!=EmployeeJobPositionAssignmentStatus.Cancelled && (!x.EndDate.HasValue||x.EndDate.Value>plannedEndDate));
   int contracts=e.EmploymentContracts.Count(x=>x.Status is not (EmploymentContractStatus.Terminated or EmploymentContractStatus.Completed or EmploymentContractStatus.Cancelled) && (!x.EndDate.HasValue||x.EndDate.Value>plannedEndDate));
   int equipment=await db.EquipmentAssignments.AsNoTracking().CountAsync(x=>x.OrganizationId==org&&x.EmployeeId==employeeId&&(x.Status==EquipmentAssignmentStatus.Planned||x.Status==EquipmentAssignmentStatus.Active),ct);
   int timesheets=await db.Timesheets.AsNoTracking().CountAsync(x=>x.OrganizationId==org&&x.EmployeeId==employeeId&&(x.Status==TimesheetStatus.Submitted||x.Status==TimesheetStatus.UnderReview||x.Status==TimesheetStatus.Approved),ct);
   int restrictions=await db.ProfessionalRestrictions.AsNoTracking().CountAsync(x=>x.OrganizationId==org&&x.EmployeeId==employeeId&&(x.Status==ProfessionalRestrictionStatus.Planned||x.Status==ProfessionalRestrictionStatus.Active)&&(!x.EndDate.HasValue||x.EndDate.Value>plannedEndDate),ct);
   return new(branches,positions,contracts,equipment,timesheets,restrictions);
 }
}
