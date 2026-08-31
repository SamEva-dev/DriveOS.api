using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Opportunities;
public interface IProfessionalOpportunityRepository
{
    Task<ProfessionalOpportunity?> GetAsync(ProfessionalOpportunityId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalOpportunity>> ListPublishedAsync(string? countryCode,string? categoryCode,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalOpportunity>> ListForOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default);
    void Add(ProfessionalOpportunity opportunity);
}
