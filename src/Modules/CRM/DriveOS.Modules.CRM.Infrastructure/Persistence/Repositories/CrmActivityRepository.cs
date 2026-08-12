using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class CrmActivityRepository(CrmDbContext context) : ICrmActivityRepository
{
    public void Add(CrmActivity activity) => context.Activities.Add(activity);

    public async Task<IReadOnlyList<CrmActivity>> GetByLeadAsync(
        OrganizationId organizationId, LeadId leadId, CancellationToken ct) =>
        await context.Activities.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.LeadId == leadId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<CrmActivity>> GetRecentAsync(
        OrganizationId organizationId, int limit, CancellationToken ct) =>
        await context.Activities.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(limit)
            .ToListAsync(ct);
}
