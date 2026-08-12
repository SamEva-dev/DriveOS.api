using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Domain.Offers;

public interface ICommercialOfferRepository
{
    void Add(CommercialOffer offer);
    Task<int> GetNextVersionAsync(OrganizationId organizationId, LeadId leadId, CancellationToken cancellationToken);
    Task<CommercialOffer?> GetByIdAsync(OrganizationId organizationId, CommercialOfferId offerId, CancellationToken cancellationToken);
    Task<CommercialOffer?> GetForUpdateAsync(OrganizationId organizationId, CommercialOfferId offerId, CancellationToken cancellationToken);
    Task<CommercialOffer?> GetForUpdateBySecureTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task<IReadOnlyList<CommercialOffer>> GetByLeadAsync(OrganizationId organizationId, LeadId leadId, CancellationToken cancellationToken);
}
