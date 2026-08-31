using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
public sealed class ProfessionalComplianceRequirementRepository(ProfessionalMarketplaceDbContext db):IProfessionalComplianceRequirementRepository
{
    public async Task<IReadOnlyList<ProfessionalComplianceRequirement>> ListApplicableAsync(string countryCode,ProfessionalType type,IReadOnlyCollection<string> categories,DateOnly date,CancellationToken ct=default)
    {
        countryCode=(countryCode??"").Trim().ToUpperInvariant();
        var candidates=await db.ProfessionalComplianceRequirements.AsNoTracking()
            .Where(x=>x.CountryCode==countryCode&&x.ProfessionalType==type&&x.Status==ProfessionalComplianceRequirementStatus.Active&&x.EffectiveFrom<=date&&(x.EffectiveTo==null||date<=x.EffectiveTo))
            .ToListAsync(ct);
        return candidates.Where(x=>x.AppliesOn(date,categories)).ToArray();
    }
    public Task<bool> ActiveVersionExistsAsync(string code,string country,ProfessionalType type,int version,CancellationToken ct=default)
    {
        code=(code??"").Trim().ToUpperInvariant();country=(country??"").Trim().ToUpperInvariant();
        return db.ProfessionalComplianceRequirements.AnyAsync(x=>x.RequirementCode==code&&x.CountryCode==country&&x.ProfessionalType==type&&x.Version==version,ct);
    }
    public void Add(ProfessionalComplianceRequirement x)=>db.ProfessionalComplianceRequirements.Add(x);
}
