using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ProfessionalProfileRepository(ProfessionalMarketplaceDbContext db):IProfessionalProfileRepository
{
 public Task<ProfessionalProfile?> GetByIdAsync(ProfessionalProfileId id,CancellationToken ct=default)=>db.ProfessionalProfiles.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
 public Task<ProfessionalProfile?> GetByIdForUpdateAsync(ProfessionalProfileId id,CancellationToken ct=default)=>db.ProfessionalProfiles.SingleOrDefaultAsync(x=>x.Id==id,ct);
 public Task<ProfessionalProfile?> FindByPersonAsync(PersonId personId,CancellationToken ct=default)=>db.ProfessionalProfiles.AsNoTracking().SingleOrDefaultAsync(x=>x.PersonId==personId,ct);
 public Task<ProfessionalProfile?> FindByUserAsync(UserId userId,CancellationToken ct=default)=>db.ProfessionalProfiles.AsNoTracking().SingleOrDefaultAsync(x=>x.UserId==userId,ct);
 public void Add(ProfessionalProfile profile)=>db.ProfessionalProfiles.Add(profile);
}
