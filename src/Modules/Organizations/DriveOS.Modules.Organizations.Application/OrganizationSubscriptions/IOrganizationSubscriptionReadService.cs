using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions;

public interface IOrganizationSubscriptionReadService
{
    Task<OrganizationSubscriptionResponse?> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken = default);
    Task<bool> HasEntitlementAsync(OrganizationId organizationId, string entitlementCode, CancellationToken cancellationToken = default);
    Task<long?> GetLimitAsync(OrganizationId organizationId, string limitCode, CancellationToken cancellationToken = default);
}
