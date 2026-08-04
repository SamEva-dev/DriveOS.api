using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public interface IOrganizationSubscriptionRepository
{
    Task<OrganizationSubscription?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExternalReferenceExistsAsync(
        string externalProvider,
        string externalSubscriptionId,
        OrganizationSubscriptionId? excludedId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default);
}
