using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class LeadRepository(CrmDbContext dbContext) : ILeadRepository
{
    public Task<bool> ExistsByEmailAsync(
        OrganizationId organizationId,
        string email,
        CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        return dbContext.Leads
            .AsNoTracking()
            .AnyAsync(
                lead =>
                    lead.OrganizationId == organizationId &&
                    lead.Identity.Email == normalizedEmail,
                cancellationToken);
    }

    public Task<Lead?> GetByIdAsync(
        OrganizationId organizationId,
        LeadId id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Leads
            .AsNoTracking()
            .SingleOrDefaultAsync(
                lead => lead.OrganizationId == organizationId && lead.Id == id,
                cancellationToken);
    }

    public Task<Lead?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        LeadId id,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Leads
            .SingleOrDefaultAsync(
                lead => lead.OrganizationId == organizationId && lead.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Lead entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await dbContext.Leads.AddAsync(entity, cancellationToken);
    }

}
