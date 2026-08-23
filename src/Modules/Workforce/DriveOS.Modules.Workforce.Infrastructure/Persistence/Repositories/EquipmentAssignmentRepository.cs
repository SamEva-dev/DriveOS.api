using DriveOS.Modules.Workforce.Domain.EquipmentAssignments; using DriveOS.SharedKernel.Identifiers; using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
internal sealed class EquipmentAssignmentRepository(WorkforceDbContext db):IEquipmentAssignmentRepository
{
 public Task<EquipmentAssignment?> GetAsync(OrganizationId org,EquipmentAssignmentId id,bool tracking,CancellationToken ct=default){IQueryable<EquipmentAssignment> q=db.EquipmentAssignments; if(!tracking)q=q.AsNoTracking(); return q.SingleOrDefaultAsync(x=>x.OrganizationId==org&&x.Id==id,ct);}
 public async Task<IReadOnlyList<EquipmentAssignment>> ListAsync(OrganizationId org,EmployeeId? employeeId,EquipmentAssignmentStatus? status,EquipmentResourceType? type,CancellationToken ct=default){var q=db.EquipmentAssignments.AsNoTracking().Where(x=>x.OrganizationId==org);if(employeeId.HasValue)q=q.Where(x=>x.EmployeeId==employeeId.Value);if(status.HasValue)q=q.Where(x=>x.Status==status.Value);if(type.HasValue)q=q.Where(x=>x.ResourceType==type.Value);return await q.OrderByDescending(x=>x.StartDate).ToListAsync(ct);}
 public Task<bool> HasResourceOverlapAsync(OrganizationId org,EquipmentResourceType type,Guid resourceId,DateOnly from,DateOnly? to,EquipmentAssignmentId? excluding,CancellationToken ct=default){var max=to??DateOnly.MaxValue;return db.EquipmentAssignments.AsNoTracking().AnyAsync(x=>x.OrganizationId==org&&x.ResourceType==type&&x.ResourceId==resourceId&&x.Status!=EquipmentAssignmentStatus.Returned&&x.Status!=EquipmentAssignmentStatus.Cancelled&&(!excluding.HasValue||x.Id!=excluding.Value)&&x.StartDate<=max&&(x.PlannedEndDate==null||x.PlannedEndDate>=from),ct);}
 public void Add(EquipmentAssignment x)=>db.EquipmentAssignments.Add(x);
}
