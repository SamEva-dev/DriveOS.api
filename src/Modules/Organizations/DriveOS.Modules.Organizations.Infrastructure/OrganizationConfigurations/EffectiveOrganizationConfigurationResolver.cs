using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationConfigurations;

internal sealed class EffectiveOrganizationConfigurationResolver(
    OrganizationsDbContext dbContext,
    IClock clock,
    OrganizationConfigurationMemoryCache cache,
    IBranchConfigurationMergePolicy mergePolicy,
    IJsonConfigurationMerger jsonMerger
) : IEffectiveOrganizationConfigurationResolver
{
    public async Task<EffectiveOrganizationConfiguration?> ResolveCurrentAsync(
        OrganizationId organizationId,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default
    )
    {
        if (cache.TryGet(organizationId, branchId, out EffectiveOrganizationConfiguration? cached))
            return cached;

        EffectiveOrganizationConfiguration? resolved = await ResolveAtCoreAsync(
            organizationId,
            clock.UtcNow,
            branchId,
            cancellationToken
        );

        cache.Set(organizationId, branchId, resolved);
        return resolved;
    }

    public Task<EffectiveOrganizationConfiguration?> ResolveAtAsync(
        OrganizationId organizationId,
        DateTimeOffset instantUtc,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default
    ) => ResolveAtCoreAsync(organizationId, instantUtc, branchId, cancellationToken);

    private async Task<EffectiveOrganizationConfiguration?> ResolveAtCoreAsync(
        OrganizationId organizationId,
        DateTimeOffset instantUtc,
        BranchId? branchId,
        CancellationToken cancellationToken
    )
    {
        OrganizationProjection? organizationConfiguration = await dbContext
            .OrganizationConfigurations.AsNoTracking()
            .Where(configuration =>
                configuration.OrganizationId == organizationId
                && configuration.Status == OrganizationConfigurationStatus.Published
                && configuration.EffectiveFromUtc != null
                && configuration.EffectiveFromUtc <= instantUtc
                && (
                    configuration.EffectiveToUtc == null
                    || configuration.EffectiveToUtc > instantUtc
                )
            )
            .OrderByDescending(configuration => configuration.EffectiveFromUtc)
            .ThenByDescending(configuration => configuration.VersionNumber)
            .Select(configuration => new OrganizationProjection(
                configuration.Id.Value,
                configuration.OrganizationId,
                configuration.VersionNumber,
                configuration.CountryCode,
                configuration.Payload.Json,
                configuration.EffectiveFromUtc!.Value,
                configuration.EffectiveToUtc
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (organizationConfiguration is null)
            return null;

        if (branchId is null)
            return ToOrganizationResult(organizationConfiguration, null);

        BranchOverrideProjection? branchOverride = await dbContext
            .BranchConfigurationOverrides.AsNoTracking()
            .Where(branchConfiguration =>
                branchConfiguration.OrganizationId == organizationId
                && branchConfiguration.BranchId == branchId
                && branchConfiguration.BaseConfigurationId.Value
                    == organizationConfiguration.ConfigurationId
                && branchConfiguration.Status == BranchConfigurationOverrideStatus.Published
                && branchConfiguration.EffectiveFromUtc != null
                && branchConfiguration.EffectiveFromUtc <= instantUtc
                && (
                    branchConfiguration.EffectiveToUtc == null
                    || branchConfiguration.EffectiveToUtc > instantUtc
                )
            )
            .OrderByDescending(branchConfiguration => branchConfiguration.EffectiveFromUtc)
            .ThenByDescending(branchConfiguration => branchConfiguration.VersionNumber)
            .Select(branchConfiguration => new BranchOverrideProjection(
                branchConfiguration.Id.Value,
                branchConfiguration.VersionNumber,
                branchConfiguration.Payload.Json,
                branchConfiguration.EffectiveFromUtc!.Value,
                branchConfiguration.EffectiveToUtc
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (branchOverride is null)
            return ToOrganizationResult(organizationConfiguration, branchId);

        BranchConfigurationMergePolicyResult policyResult = mergePolicy.Validate(
            branchOverride.OverrideJson
        );
        if (!policyResult.IsAllowed)
        {
            // Invalid historical data must not weaken the effective organization configuration.
            // Publication-time validation should normally prevent this fallback.
            return ToOrganizationResult(organizationConfiguration, branchId);
        }

        string mergedPayload = jsonMerger.Merge(
            organizationConfiguration.PayloadJson,
            branchOverride.OverrideJson
        );

        return new EffectiveOrganizationConfiguration(
            branchOverride.OverrideId,
            organizationConfiguration.OrganizationId,
            branchId,
            organizationConfiguration.VersionNumber,
            organizationConfiguration.CountryCode,
            mergedPayload,
            Max(organizationConfiguration.EffectiveFromUtc, branchOverride.EffectiveFromUtc),
            Min(organizationConfiguration.EffectiveToUtc, branchOverride.EffectiveToUtc),
            OrganizationConfigurationSource.BranchOverride,
            organizationConfiguration.ConfigurationId,
            branchOverride.VersionNumber
        );
    }

    private static EffectiveOrganizationConfiguration ToOrganizationResult(
        OrganizationProjection configuration,
        BranchId? branchId
    ) =>
        new(
            configuration.ConfigurationId,
            configuration.OrganizationId,
            branchId,
            configuration.VersionNumber,
            configuration.CountryCode,
            configuration.PayloadJson,
            configuration.EffectiveFromUtc,
            configuration.EffectiveToUtc,
            OrganizationConfigurationSource.Organization,
            configuration.ConfigurationId,
            null
        );

    private static DateTimeOffset Max(DateTimeOffset left, DateTimeOffset right) =>
        left >= right ? left : right;

    private static DateTimeOffset? Min(DateTimeOffset? left, DateTimeOffset? right)
    {
        if (left is null)
            return right;
        if (right is null)
            return left;
        return left <= right ? left : right;
    }

    private sealed record OrganizationProjection(
        Guid ConfigurationId,
        OrganizationId OrganizationId,
        int VersionNumber,
        string CountryCode,
        string PayloadJson,
        DateTimeOffset EffectiveFromUtc,
        DateTimeOffset? EffectiveToUtc
    );

    private sealed record BranchOverrideProjection(
        Guid OverrideId,
        int VersionNumber,
        string OverrideJson,
        DateTimeOffset EffectiveFromUtc,
        DateTimeOffset? EffectiveToUtc
    );
}
