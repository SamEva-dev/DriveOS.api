using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
public interface IProfessionalCommercialOfferRepository
{
    Task<ProfessionalCommercialOffer?> GetAsync(ProfessionalCommercialOfferId id,bool tracking,CancellationToken ct=default);
    Task<bool> ActiveOfferExistsForApplicationAsync(ProfessionalApplicationId applicationId,CancellationToken ct=default);
    Task<bool> ActiveOfferExistsForProposalAsync(ProfessionalProposalId proposalId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalCommercialOffer>> ListAsync(
        OrganizationId organizationId,
        ProfessionalProfileId professionalProfileId,
        ProfessionalApplicationId? applicationId,
        ProfessionalProposalId? proposalId,
        ProfessionalOpportunityId? opportunityId,
        CancellationToken ct=default);
    void Add(ProfessionalCommercialOffer offer);
}
