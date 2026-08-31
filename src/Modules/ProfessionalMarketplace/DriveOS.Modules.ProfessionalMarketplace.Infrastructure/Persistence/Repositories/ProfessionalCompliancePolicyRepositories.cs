using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalCompliancePolicyRepository(
    ProfessionalMarketplaceDbContext db):IProfessionalCompliancePolicyRepository
{
    public Task<ProfessionalComplianceCriticalityPolicy?> GetAsync(
        ProfessionalCompliancePolicyId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.ProfessionalComplianceCriticalityPolicies.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.ProfessionalComplianceCriticalityPolicies.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<ProfessionalComplianceCriticalityPolicy?> GetApplicableAsync(
        string countryCode,string requirementCode,DateOnly date,CancellationToken ct=default)
    {
        countryCode=(countryCode??string.Empty).Trim().ToUpperInvariant();
        requirementCode=(requirementCode??string.Empty).Trim().ToUpperInvariant();

        return db.ProfessionalComplianceCriticalityPolicies.AsNoTracking()
            .Where(x=>x.CountryCode==countryCode&&x.RequirementCode==requirementCode&&
                x.Status==ProfessionalCompliancePolicyStatus.Active&&x.EffectiveFrom<=date&&
                (x.EffectiveTo==null||date<=x.EffectiveTo.Value))
            .OrderByDescending(x=>x.Version)
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> ExistsVersionAsync(
        string countryCode,string requirementCode,int version,CancellationToken ct=default)
    {
        countryCode=(countryCode??string.Empty).Trim().ToUpperInvariant();
        requirementCode=(requirementCode??string.Empty).Trim().ToUpperInvariant();
        return db.ProfessionalComplianceCriticalityPolicies.AsNoTracking()
            .AnyAsync(x=>x.CountryCode==countryCode&&x.RequirementCode==requirementCode&&x.Version==version,ct);
    }

    public async Task<IReadOnlyList<ProfessionalComplianceCriticalityPolicy>> ListAsync(
        string? countryCode,CancellationToken ct=default)
    {
        IQueryable<ProfessionalComplianceCriticalityPolicy> q=db.ProfessionalComplianceCriticalityPolicies.AsNoTracking();
        if(!string.IsNullOrWhiteSpace(countryCode))
        {
            string country=countryCode.Trim().ToUpperInvariant();
            q=q.Where(x=>x.CountryCode==country);
        }

        return await q.OrderBy(x=>x.CountryCode)
            .ThenBy(x=>x.RequirementCode)
            .ThenByDescending(x=>x.Version)
            .ToListAsync(ct);
    }

    public void Add(ProfessionalComplianceCriticalityPolicy policy)=>
        db.ProfessionalComplianceCriticalityPolicies.Add(policy);
}

internal sealed class ProfessionalComplianceWaiverRepository(
    ProfessionalMarketplaceDbContext db):IProfessionalComplianceWaiverRepository
{
    public Task<ProfessionalComplianceWaiver?> GetAsync(
        ProfessionalComplianceWaiverId id,bool tracking,CancellationToken ct=default)=>
        tracking
            ?db.ProfessionalComplianceWaivers.SingleOrDefaultAsync(x=>x.Id==id,ct)
            :db.ProfessionalComplianceWaivers.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);

    public Task<ProfessionalComplianceWaiver?> GetEffectiveAsync(
        ProfessionalProfileId profileId,string countryCode,string requirementCode,DateOnly date,CancellationToken ct=default)
    {
        countryCode=(countryCode??string.Empty).Trim().ToUpperInvariant();
        requirementCode=(requirementCode??string.Empty).Trim().ToUpperInvariant();

        return db.ProfessionalComplianceWaivers.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId&&x.CountryCode==countryCode&&
                x.RequirementCode==requirementCode&&x.Status==ProfessionalComplianceWaiverStatus.Active&&
                x.ValidFrom<=date&&date<=x.ValidUntil)
            .OrderByDescending(x=>x.ValidUntil)
            .FirstOrDefaultAsync(ct);
    }

    public Task<bool> ExistsOverlappingAsync(
        ProfessionalProfileId profileId,string requirementCode,DateOnly from,DateOnly until,CancellationToken ct=default)
    {
        requirementCode=(requirementCode??string.Empty).Trim().ToUpperInvariant();
        return db.ProfessionalComplianceWaivers.AsNoTracking().AnyAsync(x=>
            x.ProfessionalProfileId==profileId&&x.RequirementCode==requirementCode&&
            x.Status==ProfessionalComplianceWaiverStatus.Active&&x.ValidFrom<=until&&from<=x.ValidUntil,ct);
    }

    public async Task<IReadOnlyList<ProfessionalComplianceWaiver>> ListByProfileAsync(
        ProfessionalProfileId profileId,CancellationToken ct=default)=>
        await db.ProfessionalComplianceWaivers.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId)
            .OrderByDescending(x=>x.ValidUntil)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProfessionalComplianceWaiver>> ListExpiredActiveAsync(
        DateOnly today,bool tracking,CancellationToken ct=default)
    {
        IQueryable<ProfessionalComplianceWaiver> q=db.ProfessionalComplianceWaivers
            .Where(x=>x.Status==ProfessionalComplianceWaiverStatus.Active&&x.ValidUntil<today);
        if(!tracking)q=q.AsNoTracking();
        return await q.OrderBy(x=>x.ValidUntil).ToListAsync(ct);
    }

    public void Add(ProfessionalComplianceWaiver waiver)=>db.ProfessionalComplianceWaivers.Add(waiver);
}
