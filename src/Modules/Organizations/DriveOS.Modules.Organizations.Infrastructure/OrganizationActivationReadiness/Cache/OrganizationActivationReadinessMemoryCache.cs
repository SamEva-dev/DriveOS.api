using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Cache;

public sealed class OrganizationActivationReadinessMemoryCache(
    IMemoryCache memoryCache,
    IOptions<OrganizationActivationReadinessCacheOptions> options
) : IOrganizationActivationReadinessReportCache
{
    private readonly OrganizationActivationReadinessCacheOptions _options = options.Value;

    public async Task<OrganizationActivationReadinessReport> GetOrCreateAsync(
        OrganizationId organizationId,
        Func<CancellationToken, Task<OrganizationActivationReadinessReport>> factory,
        CancellationToken cancellationToken = default
    )
    {
        if (!_options.Enabled || _options.DurationSeconds <= 0)
        {
            return await factory(cancellationToken);
        }

        string key = GetKey(organizationId);

        if (
            memoryCache.TryGetValue(key, out OrganizationActivationReadinessReport? cached)
            && cached is not null
        )
        {
            return cached;
        }

        OrganizationActivationReadinessReport report = await factory(cancellationToken);

        memoryCache.Set(
            key,
            report,
            TimeSpan.FromSeconds(Math.Clamp(_options.DurationSeconds, 1, 300))
        );

        return report;
    }

    public void Invalidate(OrganizationId organizationId) =>
        memoryCache.Remove(GetKey(organizationId));

    private static string GetKey(OrganizationId organizationId) =>
        $"organizations:activation-readiness:{organizationId.Value:N}";
}
