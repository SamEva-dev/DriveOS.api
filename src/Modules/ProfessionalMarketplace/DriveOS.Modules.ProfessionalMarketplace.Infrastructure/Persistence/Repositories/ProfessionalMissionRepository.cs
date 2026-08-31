using DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;

internal sealed class ProfessionalMissionRepository(
    ProfessionalMarketplaceDbContext db) : IProfessionalMissionRepository
{
    public Task<ProfessionalMission?> GetAsync(
        ProfessionalMissionId id,
        bool tracking,
        CancellationToken ct = default) =>
        tracking
            ? db.ProfessionalMissions.SingleOrDefaultAsync(x => x.Id == id, ct)
            : db.ProfessionalMissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ProfessionalMission>> ListByEngagementAsync(
        ProfessionalEngagementId engagementId,
        CancellationToken ct = default) =>
        await db.ProfessionalMissions.AsNoTracking()
            .Where(x => x.EngagementId == engagementId)
            .OrderBy(x => x.StartsOn)
            .ThenBy(x => x.Title)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProfessionalMission>> ListByOrganizationAsync(
        OrganizationId organizationId,
        CancellationToken ct = default) =>
        await db.ProfessionalMissions.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ProfessionalMission>> ListByProfileAsync(
        ProfessionalProfileId profileId,
        CancellationToken ct=default) =>
        await db.ProfessionalMissions.AsNoTracking()
            .Where(x=>x.ProfessionalProfileId==profileId)
            .OrderByDescending(x=>x.CreatedAtUtc)
            .ToListAsync(ct);

    public void Add(ProfessionalMission mission) => db.ProfessionalMissions.Add(mission);
}
