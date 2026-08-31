using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Contracts.Domain.ProfessionalServiceContracts;
public interface IProfessionalServiceContractRepository
{
    Task<ProfessionalServiceContract?> GetAsync(ProfessionalServiceContractId id,bool tracking,CancellationToken ct=default);
    Task<ProfessionalServiceContract?> GetByEngagementAsync(ProfessionalEngagementId engagementId,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsForEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    void Add(ProfessionalServiceContract contract);
}
