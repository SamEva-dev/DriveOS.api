using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
public interface IServiceEntryRepository
{
    Task<ServiceEntry?> GetAsync(ServiceEntryId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsForSourceAsync(ProfessionalEngagementId engagementId,ServiceEntrySourceType sourceType,Guid sourceId,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceEntry>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceEntry>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceEntry>> ListByMissionAsync(ProfessionalMissionId missionId,CancellationToken ct=default);
    void Add(ServiceEntry entry);
}
