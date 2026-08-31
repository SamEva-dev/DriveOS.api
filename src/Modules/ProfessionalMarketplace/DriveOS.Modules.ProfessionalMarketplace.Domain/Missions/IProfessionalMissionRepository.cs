using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Missions;

public interface IProfessionalMissionRepository
{
    Task<ProfessionalMission?> GetAsync(ProfessionalMissionId id,bool tracking,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalMission>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalMission>> ListByOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalMission>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ProfessionalMission mission);
}
