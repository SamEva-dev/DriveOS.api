using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
public interface IProfessionalProposalRepository
{
    Task<ProfessionalProposal?> GetAsync(ProfessionalProposalId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalProposal>> ListAsync(OrganizationId organizationId,ProfessionalProfileId profileId,ProfessionalOpportunityId? opportunityId,CancellationToken ct=default);
    Task<bool> OpenProposalExistsAsync(OrganizationId organizationId,ProfessionalProfileId profileId,ProfessionalOpportunityId? opportunityId,CancellationToken ct=default);
    void Add(ProfessionalProposal proposal);
}
