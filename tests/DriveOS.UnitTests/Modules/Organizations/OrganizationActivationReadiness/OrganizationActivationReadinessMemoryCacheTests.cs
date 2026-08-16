using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Cache;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DriveOS.UnitTests.OrganizationActivationReadiness;

public sealed class OrganizationActivationReadinessMemoryCacheTests
{
    [Fact]
    public async Task GetOrCreateAsync_ShouldReuseReport_UntilInvalidated()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var options = Options.Create(
            new OrganizationActivationReadinessCacheOptions { Enabled = true, DurationSeconds = 60 }
        );
        var cache = new OrganizationActivationReadinessMemoryCache(memoryCache, options);
        OrganizationId organizationId = new(Guid.NewGuid());
        int factoryCalls = 0;

        Task<OrganizationActivationReadinessReport> Factory(CancellationToken _)
        {
            factoryCalls++;
            return Task.FromResult(
                new OrganizationActivationReadinessReport(organizationId, true, [])
            );
        }

        await cache.GetOrCreateAsync(organizationId, Factory);
        await cache.GetOrCreateAsync(organizationId, Factory);
        cache.Invalidate(organizationId);
        await cache.GetOrCreateAsync(organizationId, Factory);

        factoryCalls.Should().Be(2);
    }
}
