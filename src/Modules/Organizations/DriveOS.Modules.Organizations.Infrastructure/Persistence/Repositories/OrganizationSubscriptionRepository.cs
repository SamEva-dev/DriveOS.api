using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Repositories;

internal sealed class OrganizationSubscriptionRepository(OrganizationsDbContext dbContext)
    : IOrganizationSubscriptionRepository
{
    public async Task<OrganizationSubscription?> GetForUpdateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .OrganizationSubscriptions.Include(subscription => subscription.Entitlements)
            .Include(subscription => subscription.Limits)
            .SingleOrDefaultAsync(
                subscription => subscription.OrganizationId == organizationId,
                cancellationToken
            );
    }

    public Task<bool> ExistsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .OrganizationSubscriptions.AsNoTracking()
            .AnyAsync(
                subscription => subscription.OrganizationId == organizationId,
                cancellationToken
            );
    }

    public Task<bool> ExternalReferenceExistsAsync(
        string externalProvider,
        string externalSubscriptionId,
        OrganizationSubscriptionId? excludedId = null,
        CancellationToken cancellationToken = default
    )
    {
        string normalizedProvider = externalProvider.Trim();
        string normalizedReference = externalSubscriptionId.Trim();

        return dbContext
            .OrganizationSubscriptions.AsNoTracking()
            .AnyAsync(
                subscription =>
                    subscription.ExternalProvider == normalizedProvider
                    && subscription.ExternalSubscriptionId == normalizedReference
                    && (!excludedId.HasValue || subscription.Id != excludedId.Value),
                cancellationToken
            );
    }

    public async Task AddAsync(
        OrganizationSubscription subscription,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(subscription);
        await dbContext.OrganizationSubscriptions.AddAsync(subscription, cancellationToken);
    }
}
