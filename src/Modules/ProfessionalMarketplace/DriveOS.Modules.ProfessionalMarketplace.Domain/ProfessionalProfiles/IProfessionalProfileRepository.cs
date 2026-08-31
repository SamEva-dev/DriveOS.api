using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
public interface IProfessionalProfileRepository
{
    Task<ProfessionalProfile?> GetByIdAsync(ProfessionalProfileId id,CancellationToken ct=default);
    Task<ProfessionalProfile?> GetByIdForUpdateAsync(ProfessionalProfileId id,CancellationToken ct=default);
    Task<ProfessionalProfile?> FindByPersonAsync(PersonId personId,CancellationToken ct=default);
    Task<ProfessionalProfile?> FindByUserAsync(UserId userId,CancellationToken ct=default);
    void Add(ProfessionalProfile profile);
}
