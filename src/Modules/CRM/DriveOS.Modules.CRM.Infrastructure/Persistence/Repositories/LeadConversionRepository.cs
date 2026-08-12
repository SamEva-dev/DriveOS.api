using DriveOS.Modules.CRM.Domain.Conversions;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class LeadConversionRepository(CrmDbContext dbContext)
    : ILeadConversionRepository
{
    public Task<LeadConversion?> GetByLeadIdAsync(OrganizationId organizationId,
        LeadId leadId, CancellationToken cancellationToken = default) =>
        dbContext.LeadConversions.AsNoTracking().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.LeadId == leadId,
            cancellationToken);

    public Task AddAsync(LeadConversion conversion,
        CancellationToken cancellationToken = default) =>
        dbContext.LeadConversions.AddAsync(conversion, cancellationToken).AsTask();
}
