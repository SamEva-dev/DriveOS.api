using DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

/// <summary>
/// Builds the closure snapshot from data owned by Organization & Tenancy.
/// External bounded-context counters are deliberately left at zero until their
/// adapters are connected; see README for the required integration points.
/// </summary>
internal sealed class OrganizationClosureReadinessSnapshotSource(OrganizationsDbContext dbContext)
    : IOrganizationClosureReadinessSnapshotSource
{
    public async Task<OrganizationClosureReadinessSnapshot> GetAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken
    )
    {
        bool organizationExists = await dbContext
            .Organizations.AsNoTracking()
            .AnyAsync(x => x.Id == organizationId, cancellationToken);

        int activeBranches = await dbContext
            .Branches.AsNoTracking()
            .CountAsync(
                x => x.OrganizationId == organizationId && x.Status != BranchStatus.Closed,
                cancellationToken
            );

        int activePrivilegedMemberships = await dbContext
            .OrganizationRepresentatives.AsNoTracking()
            .CountAsync(
                x =>
                    x.OrganizationId == organizationId
                    && x.Status == OrganizationRepresentativeStatus.Active,
                cancellationToken
            );

        bool hasActiveSubscription = await dbContext
            .OrganizationSubscriptions.AsNoTracking()
            .AnyAsync(
                x =>
                    x.OrganizationId == organizationId
                    && (
                        x.Status == SubscriptionStatus.Active
                        || x.Status == SubscriptionStatus.Trialing
                        || x.Status == SubscriptionStatus.PastDue
                        || x.Status == SubscriptionStatus.Restricted
                        || x.Status == SubscriptionStatus.Suspended
                    ),
                cancellationToken
            );

        bool retentionPolicyConfigured = await dbContext
            .OrganizationSettings.AsNoTracking()
            .AnyAsync(x => x.OrganizationId == organizationId, cancellationToken);

        return new OrganizationClosureReadinessSnapshot(
            OrganizationExists: organizationExists,
            ActiveBranches: activeBranches,
            ActivePrivilegedMemberships: activePrivilegedMemberships,
            HasActiveSubscription: hasActiveSubscription,
            OpenOperations: 0,
            BlockingFinancialItems: 0,
            ActiveIntegrations: 0,
            RetentionPolicyConfigured: retentionPolicyConfigured
        );
    }
}
