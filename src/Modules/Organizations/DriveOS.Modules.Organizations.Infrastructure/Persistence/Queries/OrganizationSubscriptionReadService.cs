using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Organizations.Infrastructure.Persistence.Queries;

internal sealed class OrganizationSubscriptionReadService(OrganizationsDbContext dbContext) : IOrganizationSubscriptionReadService
{
    public async Task<OrganizationSubscriptionResponse?> GetByOrganizationIdAsync(OrganizationId organizationId, CancellationToken cancellationToken = default)
    {
        var subscription = await dbContext.OrganizationSubscriptions.AsNoTracking().Include(x => x.Entitlements).Include(x => x.Limits).SingleOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);
        if (subscription is null) return null;
        return new OrganizationSubscriptionResponse(
            subscription.Id.Value, subscription.OrganizationId.Value, subscription.PlanCode.Value, (int)subscription.Status, (int)subscription.BillingCycle,
            new SubscriptionPeriodResponse(subscription.CurrentPeriod.StartsAtUtc, subscription.CurrentPeriod.EndsAtUtc),
            subscription.TrialPeriod is null ? null : new SubscriptionPeriodResponse(subscription.TrialPeriod.StartsAtUtc, subscription.TrialPeriod.EndsAtUtc),
            subscription.Cancellation is null ? null : new SubscriptionCancellationResponse(subscription.Cancellation.RequestedAtUtc, subscription.Cancellation.EffectiveAtUtc, subscription.Cancellation.Reason, subscription.Cancellation.RequestedByUserId.Value),
            subscription.ExternalProvider, subscription.ExternalSubscriptionId,
            subscription.Entitlements.Select(x => new SubscriptionEntitlementResponse(x.Code)).ToArray(),
            subscription.Limits.Select(x => new SubscriptionLimitResponse(x.Code, x.Value)).ToArray(),
            subscription.Version, subscription.CreatedAtUtc, subscription.LastModifiedAtUtc);
    }
    public Task<bool> HasEntitlementAsync(OrganizationId organizationId, string entitlementCode, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationSubscriptions.AsNoTracking().Where(x => x.OrganizationId == organizationId).SelectMany(x => x.Entitlements).AnyAsync(x => x.Code == entitlementCode.Trim(), cancellationToken);
    public Task<long?> GetLimitAsync(OrganizationId organizationId, string limitCode, CancellationToken cancellationToken = default) =>
        dbContext.OrganizationSubscriptions.AsNoTracking().Where(x => x.OrganizationId == organizationId).SelectMany(x => x.Limits).Where(x => x.Code == limitCode.Trim()).Select(x => (long?)x.Value).SingleOrDefaultAsync(cancellationToken);
}
