using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationConfigurations;

internal sealed class EffectiveOrganizationConfigurationResolver(
    OrganizationsDbContext dbContext,
    IClock clock,
    OrganizationConfigurationMemoryCache cache)
    : IEffectiveOrganizationConfigurationResolver
{
    public async Task<EffectiveOrganizationConfiguration?> ResolveCurrentAsync(
        OrganizationId organizationId,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGet(organizationId, branchId, out EffectiveOrganizationConfiguration? cached))
            return cached;

        EffectiveOrganizationConfiguration? resolved = await ResolveAtCoreAsync(
            organizationId, clock.UtcNow, branchId, cancellationToken);

        cache.Set(organizationId, branchId, resolved);
        return resolved;
    }

    public Task<EffectiveOrganizationConfiguration?> ResolveAtAsync(
        OrganizationId organizationId,
        DateTimeOffset instantUtc,
        BranchId? branchId = null,
        CancellationToken cancellationToken = default) =>
        ResolveAtCoreAsync(organizationId, instantUtc, branchId, cancellationToken);

    private Task<EffectiveOrganizationConfiguration?> ResolveAtCoreAsync(
        OrganizationId organizationId,
        DateTimeOffset instantUtc,
        BranchId? branchId,
        CancellationToken cancellationToken)
    {
        // ORG-010 branch overrides will be resolved before the organization fallback.
        // Until then, branchId participates in the cache key but does not alter the source.
        return dbContext.OrganizationConfigurations
            .AsNoTracking()
            .Where(configuration =>
                configuration.OrganizationId == organizationId &&
                configuration.Status == OrganizationConfigurationStatus.Published &&
                configuration.EffectiveFromUtc != null &&
                configuration.EffectiveFromUtc <= instantUtc &&
                (configuration.EffectiveToUtc == null || configuration.EffectiveToUtc > instantUtc))
            .OrderByDescending(configuration => configuration.EffectiveFromUtc)
            .ThenByDescending(configuration => configuration.VersionNumber)
            .Select(configuration => new EffectiveOrganizationConfiguration(
                configuration.Id.Value,
                configuration.OrganizationId,
                branchId,
                configuration.VersionNumber,
                configuration.CountryCode,
                configuration.Payload.Json,
                configuration.EffectiveFromUtc!.Value,
                configuration.EffectiveToUtc,
                OrganizationConfigurationSource.Organization))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
