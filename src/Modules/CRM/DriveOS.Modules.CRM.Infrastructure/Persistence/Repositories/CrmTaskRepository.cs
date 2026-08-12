using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class CrmTaskRepository(CrmDbContext context) : ICrmTaskRepository
{
    public void Add(CrmTask task) => context.Tasks.Add(task);
    public Task<CrmTask?> GetByIdForUpdateAsync(OrganizationId organizationId, CrmTaskId taskId, CancellationToken ct) =>
        context.Tasks.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == taskId, ct);
    public async Task<IReadOnlyList<CrmTask>> GetByLeadAsync(OrganizationId organizationId, LeadId leadId, CancellationToken ct) =>
        await context.Tasks.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.LeadId == leadId)
            .OrderBy(x => x.Status).ThenBy(x => x.DueAtUtc).ToListAsync(ct);
    public async Task<IReadOnlyList<CrmTask>> GetPendingAsync(OrganizationId organizationId, CancellationToken ct) =>
        await context.Tasks.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.Status == CrmTaskStatus.Pending)
            .OrderBy(x => x.DueAtUtc).Take(200).ToListAsync(ct);
}
