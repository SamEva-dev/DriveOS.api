using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
public interface IExternalAccessGrantRepository
{
    Task<ExternalAccessGrant?> GetAsync(ExternalAccessGrantId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsActiveAsync(ProfessionalEngagementId engagementId,ExternalAccessResourceType resourceType,Guid resourceId,string permission,CancellationToken ct=default);
    Task<IReadOnlyList<ExternalAccessGrant>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<bool> HasEffectiveGrantAsync(ProfessionalProfileId profileId,OrganizationId organizationId,ExternalAccessResourceType resourceType,Guid resourceId,string permission,DateOnly date,CancellationToken ct=default);
    void Add(ExternalAccessGrant grant);
}
