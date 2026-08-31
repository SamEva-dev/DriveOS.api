using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;

public interface IServiceDisputeRepository
{
    Task<ServiceDispute?> GetAsync(ServiceDisputeId id,bool tracking,CancellationToken ct=default);
    Task<bool> HasOpenDisputeAsync(ServiceEntryId serviceEntryId,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceDispute>> ListByOrganizationAsync(OrganizationId organizationId,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceDispute>> ListByProfessionalAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ServiceDispute dispute);
}
