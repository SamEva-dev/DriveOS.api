using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Offers;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Repositories;

internal sealed class CommercialOfferRepository(CrmDbContext context) : ICommercialOfferRepository
{
    public void Add(CommercialOffer offer) => context.CommercialOffers.Add(offer);

    public async Task<int> GetNextVersionAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken
    )
    {
        int? current = await context
            .CommercialOffers.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.LeadId == leadId)
            .MaxAsync(x => (int?)x.Version, cancellationToken);
        return (current ?? 0) + 1;
    }

    public Task<CommercialOffer?> GetByIdAsync(
        OrganizationId organizationId,
        CommercialOfferId offerId,
        CancellationToken cancellationToken
    ) =>
        context
            .CommercialOffers.AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Interactions)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == offerId,
                cancellationToken
            );

    public Task<CommercialOffer?> GetForUpdateAsync(
        OrganizationId organizationId,
        CommercialOfferId offerId,
        CancellationToken cancellationToken
    ) =>
        context
            .CommercialOffers.Include(x => x.Lines)
            .Include(x => x.Interactions)
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == offerId,
                cancellationToken
            );

    public Task<CommercialOffer?> GetForUpdateBySecureTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken
    ) =>
        context
            .CommercialOffers.Include(x => x.Lines)
            .Include(x => x.Interactions)
            .SingleOrDefaultAsync(x => x.SecureLinkTokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<CommercialOffer>> GetByLeadAsync(
        OrganizationId organizationId,
        LeadId leadId,
        CancellationToken cancellationToken
    ) =>
        await context
            .CommercialOffers.AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Interactions)
            .Where(x => x.OrganizationId == organizationId && x.LeadId == leadId)
            .OrderByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
}
