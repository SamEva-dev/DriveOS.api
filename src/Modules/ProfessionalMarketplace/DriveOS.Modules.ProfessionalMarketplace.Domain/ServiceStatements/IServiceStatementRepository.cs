using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
public interface IServiceStatementRepository
{
    Task<ServiceStatement?> GetAsync(ServiceStatementId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsForPeriodAsync(ProfessionalEngagementId engagementId,DateOnly periodStart,DateOnly periodEnd,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceStatement>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<IReadOnlyList<ServiceStatement>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ServiceStatement statement);
}
