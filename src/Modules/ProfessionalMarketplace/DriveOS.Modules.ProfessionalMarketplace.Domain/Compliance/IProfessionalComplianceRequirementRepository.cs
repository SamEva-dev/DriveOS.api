using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
public interface IProfessionalComplianceRequirementRepository
{
    Task<IReadOnlyList<ProfessionalComplianceRequirement>> ListApplicableAsync(string countryCode,ProfessionalType professionalType,IReadOnlyCollection<string> categoryCodes,DateOnly date,CancellationToken ct=default);
    Task<bool> ActiveVersionExistsAsync(string requirementCode,string countryCode,ProfessionalType professionalType,int version,CancellationToken ct=default);
    void Add(ProfessionalComplianceRequirement requirement);
}
