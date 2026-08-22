using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence.Repositories;
internal sealed class ExamAttestationRepository(ExamsCertificationDbContext db):IExamAttestationRepository
{
 private IQueryable<ExamAttestation> Q(bool tracking=false)=>tracking?db.ExamAttestations.Include(x=>x.Revisions):db.ExamAttestations.AsNoTracking().Include(x=>x.Revisions);
 public Task<ExamAttestation?> GetAsync(OrganizationId o,ExamAttestationId id,CancellationToken ct=default)=>Q().SingleOrDefaultAsync(x=>x.OrganizationId==o&&x.Id==id,ct);
 public Task<ExamAttestation?> GetForUpdateAsync(OrganizationId o,ExamAttestationId id,CancellationToken ct=default)=>Q(true).SingleOrDefaultAsync(x=>x.OrganizationId==o&&x.Id==id,ct);
 public Task<ExamAttestation?> GetCurrentAsync(OrganizationId o,ExamResultId r,ExamAttestationType t,CancellationToken ct=default)=>Q(true).SingleOrDefaultAsync(x=>x.OrganizationId==o&&x.ExamResultId==r&&x.Type==t&&x.Status!=ExamAttestationStatus.Revoked&&x.Status!=ExamAttestationStatus.Superseded&&x.Status!=ExamAttestationStatus.Expired,ct);
 public async Task<IReadOnlyList<ExamAttestation>> ListByResultAsync(OrganizationId o,ExamResultId r,CancellationToken ct=default)=>await Q().Where(x=>x.OrganizationId==o&&x.ExamResultId==r).OrderBy(x=>x.Type).ThenByDescending(x=>x.IssuedAtUtc).ToListAsync(ct);
 public async Task<IReadOnlyList<ExamAttestation>> ListByResultRevisionForUpdateAsync(OrganizationId o,ExamResultId r,int v,CancellationToken ct=default)=>await Q(true).Where(x=>x.OrganizationId==o&&x.ExamResultId==r&&x.ResultRevision==v&&x.Status!=ExamAttestationStatus.Revoked&&x.Status!=ExamAttestationStatus.Superseded).ToListAsync(ct);
 public async Task<IReadOnlyList<ExamAttestation>> ListByStudentAsync(OrganizationId o,PersonId s,CancellationToken ct=default)=>await Q().Where(x=>x.OrganizationId==o&&x.StudentId==s).OrderByDescending(x=>x.IssuedAtUtc).ToListAsync(ct);
 public Task<ExamAttestation?> FindByOperationIdAsync(OrganizationId o,Guid op,CancellationToken ct=default)=>Q().SingleOrDefaultAsync(x=>x.OrganizationId==o&&x.OperationId==op,ct);
 public Task<ExamAttestation?> FindByPublicVerificationTokenHashAsync(string h,CancellationToken ct=default)=>Q().SingleOrDefaultAsync(x=>x.Revisions.Any(r=>r.PublicVerificationTokenHash==h),ct);
 public void Add(ExamAttestation x)=>db.ExamAttestations.Add(x);
}
