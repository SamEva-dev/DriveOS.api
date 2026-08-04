using System.Collections.Concurrent;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Effective;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.Extensions.Configuration;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationConfigurations;

internal sealed class OrganizationConfigurationMemoryCache(IConfiguration configuration)
    : IOrganizationConfigurationCacheInvalidator
{
    private readonly ConcurrentDictionary<CacheKey, CacheEntry> _entries = new();
    private readonly TimeSpan _ttl = TimeSpan.FromSeconds(
        Math.Clamp(configuration.GetValue<int?>("OrganizationConfigurations:CacheTtlSeconds") ?? 300, 30, 3600));

    public bool TryGet(OrganizationId organizationId, BranchId? branchId,
        out EffectiveOrganizationConfiguration? value)
    {
        var key = new CacheKey(organizationId.Value, branchId?.Value);
        if (_entries.TryGetValue(key, out CacheEntry? entry) && entry.ExpiresAtUtc > DateTimeOffset.UtcNow)
        {
            value = entry.Value;
            return true;
        }

        _entries.TryRemove(key, out _);
        value = null;
        return false;
    }

    public void Set(OrganizationId organizationId, BranchId? branchId,
        EffectiveOrganizationConfiguration? value) =>
        _entries[new CacheKey(organizationId.Value, branchId?.Value)] =
            new CacheEntry(value, DateTimeOffset.UtcNow.Add(_ttl));

    public void Invalidate(OrganizationId organizationId)
    {
        foreach (CacheKey key in _entries.Keys.Where(key => key.OrganizationId == organizationId.Value))
            _entries.TryRemove(key, out _);
    }

    private sealed record CacheKey(Guid OrganizationId, Guid? BranchId);
    private sealed record CacheEntry(EffectiveOrganizationConfiguration? Value, DateTimeOffset ExpiresAtUtc);
}
