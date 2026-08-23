using DriveOS.Modules.Workforce.Application.WorkingTime;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Api.Integrations.Workforce;
internal sealed class WorkingTimeProjectionGateway(WorkforceDbContext workforceDb,SchedulingCapacityDbContext schedulingDb,TrainingDeliveryDbContext trainingDb):IWorkingTimeProjectionGateway
{
 public async Task<WorkingTimeProjectionSnapshot> GetAsync(OrganizationId organizationId,EmployeeId employeeId,DateOnly from,DateOnly to,CancellationToken ct=default)
 {
   var employee=await workforceDb.Employees.AsNoTracking().SingleOrDefaultAsync(x=>x.EmployerOrganizationId==organizationId&&x.Id==employeeId,ct);
   if(employee?.UserId is not { } userId)return new(0,0,0);
   DateTimeOffset fromUtc=new(from.ToDateTime(TimeOnly.MinValue),TimeSpan.Zero); DateTimeOffset toUtc=new(to.AddDays(1).ToDateTime(TimeOnly.MinValue),TimeSpan.Zero);
   Guid? resourceId=await schedulingDb.CalendarResources.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&x.ResourceType==CalendarResourceType.Instructor&&x.ExternalResourceId==userId.Value).Select(x=>(Guid?)x.Id.Value).FirstOrDefaultAsync(ct);
   decimal planned=0;
   if(resourceId.HasValue){var bookingIds=await schedulingDb.BookingResources.AsNoTracking().Where(x=>x.CalendarResourceId.Value==resourceId.Value).Select(x=>x.BookingId).ToListAsync(ct);var periods=await schedulingDb.Bookings.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&bookingIds.Contains(x.Id)&&x.Status!=BookingStatus.Cancelled&&x.StartAtUtc<toUtc&&x.EndAtUtc>fromUtc).Select(x=>new{x.StartAtUtc,x.EndAtUtc}).ToListAsync(ct);planned=periods.Sum(x=>(decimal)(x.EndAtUtc-x.StartAtUtc).TotalHours);}
   var sessions=await trainingDb.TrainingSessions.AsNoTracking().Where(x=>x.PerformingOrganizationId==organizationId&&x.ActualInstructorId==userId&&x.Status==TrainingSessionStatus.Completed&&x.ActualStartAtUtc<toUtc&&x.ActualEndAtUtc>fromUtc).Select(x=>x.DeliveredDurationMinutes).ToListAsync(ct);
   decimal actual=sessions.Where(x=>x.HasValue).Sum(x=>x!.Value)/60m;
   var leave=await workforceDb.LeaveRequests.AsNoTracking().Where(x=>x.OrganizationId==organizationId&&x.EmployeeId==employeeId&&x.Status==DriveOS.Modules.Workforce.Domain.LeaveRequests.LeaveRequestStatus.Approved&&x.StartDate<=to&&x.EndDate>=from).Select(x=>new{x.StartDate,x.EndDate,x.StartPortion,x.EndPortion}).ToListAsync(ct);
   decimal leaveHours=0; foreach(var l in leave){var s=l.StartDate<from?from:l.StartDate;var e=l.EndDate>to?to:l.EndDate;decimal days=e.DayNumber-s.DayNumber+1;if(s==e&&(l.StartPortion!=DriveOS.Modules.Workforce.Domain.LeaveRequests.LeaveDayPortion.FullDay||l.EndPortion!=DriveOS.Modules.Workforce.Domain.LeaveRequests.LeaveDayPortion.FullDay))days=.5m;leaveHours+=days*7m;}
   return new(decimal.Round(planned,2),decimal.Round(actual,2),decimal.Round(leaveHours,2));
 }
}
