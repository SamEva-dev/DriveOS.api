using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
public interface IProfessionalEngagementRepository
{
    Task<ProfessionalEngagement?> GetAsync(ProfessionalEngagementId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsForCommercialOfferAsync(ProfessionalCommercialOfferId offerId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalEngagement>> ListByOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalEngagement>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ProfessionalEngagement engagement);
}
