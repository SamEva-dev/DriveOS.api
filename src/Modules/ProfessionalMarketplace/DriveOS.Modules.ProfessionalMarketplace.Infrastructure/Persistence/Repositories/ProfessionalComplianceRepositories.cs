using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
public sealed class ProfessionalDocumentRepository(ProfessionalMarketplaceDbContext db):IProfessionalDocumentRepository
{
    public Task<ProfessionalDocument?> GetAsync(ProfessionalDocumentId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalDocuments.FirstOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalDocuments.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<IReadOnlyList<ProfessionalDocument>> ListByProfileAsync(ProfessionalProfileId id,CancellationToken ct=default)=>await db.ProfessionalDocuments.AsNoTracking().Where(x=>x.ProfessionalProfileId==id).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
    public Task<bool> DocumentReferenceExistsAsync(ProfessionalProfileId id,Guid reference,CancellationToken ct=default)=>db.ProfessionalDocuments.AnyAsync(x=>x.ProfessionalProfileId==id&&x.DocumentReferenceId==reference,ct);
    public async Task<IReadOnlyList<ProfessionalDocument>> ListExpirationCandidatesAsync(DateOnly today,DateOnly warningDate,bool tracking,CancellationToken ct=default)
    {
        IQueryable<ProfessionalDocument> q=db.ProfessionalDocuments.Where(x=>
            x.ExpirationDate!=null&&x.ExpirationDate<=warningDate&&
            (x.Status==ProfessionalDocumentStatus.Valid||x.Status==ProfessionalDocumentStatus.ExpiringSoon));
        if(!tracking)q=q.AsNoTracking();
        return await q.OrderBy(x=>x.ExpirationDate).ToListAsync(ct);
    }
    public void Add(ProfessionalDocument x)=>db.ProfessionalDocuments.Add(x);
}
public sealed class ProfessionalCredentialRepository(ProfessionalMarketplaceDbContext db):IProfessionalCredentialRepository
{
    public Task<ProfessionalCredential?> GetAsync(ProfessionalCredentialId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalCredentials.FirstOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalCredentials.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id,ct);
    public async Task<IReadOnlyList<ProfessionalCredential>> ListByProfileAsync(ProfessionalProfileId id,CancellationToken ct=default)=>await db.ProfessionalCredentials.AsNoTracking().Where(x=>x.ProfessionalProfileId==id).OrderBy(x=>x.CredentialTypeCode).ToListAsync(ct);
    public Task<bool> DuplicateExistsAsync(ProfessionalProfileId id,string type,string country,string? reference,CancellationToken ct=default){type=(type??"").Trim().ToUpperInvariant();country=(country??"").Trim().ToUpperInvariant();reference=string.IsNullOrWhiteSpace(reference)?null:reference.Trim();return db.ProfessionalCredentials.AnyAsync(x=>x.ProfessionalProfileId==id&&x.CredentialTypeCode==type&&x.CountryCode==country&&x.ReferenceNumber==reference&&x.Status!=ProfessionalCredentialStatus.Revoked,ct);}
    public async Task<IReadOnlyList<ProfessionalCredential>> ListExpirationCandidatesAsync(DateOnly today,DateOnly warningDate,bool tracking,CancellationToken ct=default)
    {
        IQueryable<ProfessionalCredential> q=db.ProfessionalCredentials.Where(x=>
            x.ValidUntil!=null&&x.ValidUntil<=warningDate&&
            x.Status==ProfessionalCredentialStatus.Verified);
        if(!tracking)q=q.AsNoTracking();
        return await q.OrderBy(x=>x.ValidUntil).ToListAsync(ct);
    }
    public void Add(ProfessionalCredential x)=>db.ProfessionalCredentials.Add(x);
}
