using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;
using DriveOS.Modules.Organizations.Domain.BranchAssignments;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness;

/// <summary>
/// EF Core projection used by the organization activation-readiness rules.
/// Every query is tenant-scoped, read-only and reduced to the minimum data required.
/// </summary>
internal sealed class OrganizationActivationReadinessDataSource(
    OrganizationsDbContext dbContext,
    IClock clock
) : IOrganizationActivationReadinessDataSource
{
    public Task<bool> HasActiveLegalProfileAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .OrganizationLegalProfiles.AsNoTracking()
            .AnyAsync(
                profile =>
                    profile.OrganizationId == organizationId
                    && profile.Status == OrganizationLegalProfileStatus.Active,
                cancellationToken
            );
    }

    public Task<bool> HasActiveOwnerAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        return dbContext
            .OrganizationRepresentatives.AsNoTracking()
            .AnyAsync(
                representative =>
                    representative.OrganizationId == organizationId
                    && representative.RepresentativeType == OrganizationRepresentativeType.Owner
                    && representative.Status == OrganizationRepresentativeStatus.Active
                    && representative.EffectiveFrom <= today
                    && (representative.EffectiveTo == null || representative.EffectiveTo >= today),
                cancellationToken
            );
    }

    public Task<bool> HasActivePrimaryOwnerAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        DateOnly today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        return dbContext
            .OrganizationRepresentatives.AsNoTracking()
            .AnyAsync(
                representative =>
                    representative.OrganizationId == organizationId
                    && representative.RepresentativeType == OrganizationRepresentativeType.Owner
                    && representative.IsPrimaryOwner
                    && representative.Status == OrganizationRepresentativeStatus.Active
                    && representative.EffectiveFrom <= today
                    && (representative.EffectiveTo == null || representative.EffectiveTo >= today),
                cancellationToken
            );
    }

    public Task<bool> HasActiveSubscriptionAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset now = clock.UtcNow;

        return dbContext
            .OrganizationSubscriptions.AsNoTracking()
            .AnyAsync(
                subscription =>
                    subscription.OrganizationId == organizationId
                    && (
                        subscription.Status == SubscriptionStatus.Active
                        || subscription.Status == SubscriptionStatus.Trialing
                    )
                    && subscription.CurrentPeriod.StartsAtUtc <= now
                    && (
                        subscription.CurrentPeriod.EndsAtUtc == null
                        || subscription.CurrentPeriod.EndsAtUtc > now
                    ),
                cancellationToken
            );
    }

    public Task<bool> HasOperationalSettingsAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .OrganizationSettings.AsNoTracking()
            .AnyAsync(settings => settings.OrganizationId == organizationId, cancellationToken);
    }

    public Task<BranchId?> GetPrimaryBranchIdAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        return dbContext
            .Branches.AsNoTracking()
            .Where(branch =>
                branch.OrganizationId == organizationId
                && branch.IsPrimary
                && branch.Status == BranchStatus.Active
            )
            .Select(branch => (BranchId?)branch.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<bool> HasActiveBranchManagerAsync(
        OrganizationId organizationId,
        BranchId branchId,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset now = clock.UtcNow;

        // Current branch-team model: a branch manager is represented by an active
        // Primary AdministrativeManager BranchUserAssignment. Keep the historical
        // BranchManagerAssignment lookup for backward compatibility with existing data.
        Task<bool> hasCurrentTeamManager = dbContext
            .BranchUserAssignments.AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.OrganizationId == organizationId
                    && assignment.BranchId == branchId
                    && assignment.Status == BranchUserAssignmentStatus.Active
                    && assignment.Role == BranchAssignmentRole.AdministrativeManager
                    && assignment.AssignmentType == BranchAssignmentType.Primary
                    && assignment.StartsAtUtc <= now
                    && (!assignment.PlannedEndAtUtc.HasValue || assignment.PlannedEndAtUtc > now)
                    && (!assignment.EffectiveEndAtUtc.HasValue || assignment.EffectiveEndAtUtc > now),
                cancellationToken
            );

        Task<bool> hasLegacyManager = dbContext
            .BranchManagerAssignments.AsNoTracking()
            .AnyAsync(
                assignment =>
                    assignment.BranchId == branchId
                    && assignment.Status == BranchManagerAssignmentStatus.Active
                    && assignment.EffectiveFromUtc <= now
                    && (assignment.EffectiveToUtc == null || assignment.EffectiveToUtc > now)
                    && dbContext.Branches.Any(branch =>
                        branch.Id == assignment.BranchId
                        && branch.OrganizationId == organizationId
                        && branch.Status == BranchStatus.Active
                    ),
                cancellationToken
            );

        return HasAnyManagerAsync(hasCurrentTeamManager, hasLegacyManager);
    }

    private static async Task<bool> HasAnyManagerAsync(
        Task<bool> currentTeamManagerTask,
        Task<bool> legacyManagerTask
    )
    {
        bool hasCurrentTeamManager = await currentTeamManagerTask;
        return hasCurrentTeamManager || await legacyManagerTask;
    }
}
