using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Applications;
public interface IProfessionalApplicationRepository
{
    Task<ProfessionalApplication?> GetAsync(ProfessionalApplicationId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsAsync(ProfessionalOpportunityId opportunityId,ProfessionalProfileId profileId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalApplication>> ListByOpportunityAsync(ProfessionalOpportunityId opportunityId,CancellationToken ct=default);
    void Add(ProfessionalApplication application);
}
