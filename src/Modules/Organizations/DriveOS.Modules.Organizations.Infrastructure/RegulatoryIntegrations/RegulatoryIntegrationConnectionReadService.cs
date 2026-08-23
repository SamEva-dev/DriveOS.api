using DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;
using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.RegulatoryIntegrations;

internal sealed class RegulatoryIntegrationConnectionReadService(OrganizationsDbContext dbContext)
    : IRegulatoryIntegrationConnectionReadService
{
    public async Task<IReadOnlyList<RegulatoryIntegrationConnectionResponse>> GetAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken)
        => await dbContext.RegulatoryIntegrationConnections
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId)
            .OrderBy(x => x.CountryCode)
            .ThenBy(x => x.ProviderCode)
            .ThenBy(x => x.BranchId)
            .Select(x => new RegulatoryIntegrationConnectionResponse(
                x.Id.Value,
                x.OrganizationId.Value,
                x.BranchId.HasValue ? x.BranchId.Value.Value : null,
                x.CountryCode,
                x.ProviderCode,
                x.ExternalAccountReference,
                x.SecretReference != null,
                x.Status.ToString(),
                x.Revision))
            .ToListAsync(cancellationToken);

    public async Task<RegulatoryIntegrationConnectionResponse?> ResolveActiveAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken)
    {
        RegulatoryIntegrationConnection? connection = await ResolveActiveEntityAsync(
            organizationId,
            branchId,
            countryCode,
            providerCode,
            cancellationToken);

        return connection is null
            ? null
            : new RegulatoryIntegrationConnectionResponse(
                connection.Id.Value,
                connection.OrganizationId.Value,
                connection.BranchId.HasValue ? connection.BranchId.Value.Value : null,
                connection.CountryCode,
                connection.ProviderCode,
                connection.ExternalAccountReference,
                connection.SecretReference != null,
                connection.Status.ToString(),
                connection.Revision);
    }

    public async Task<RegulatoryIntegrationTransportConnectionSnapshot?> ResolveActiveTransportAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken)
    {
        RegulatoryIntegrationConnection? connection = await ResolveActiveEntityAsync(
            organizationId,
            branchId,
            countryCode,
            providerCode,
            cancellationToken);

        return connection is null
            ? null
            : new RegulatoryIntegrationTransportConnectionSnapshot(
                connection.Id.Value,
                connection.OrganizationId.Value,
                connection.BranchId.HasValue ? connection.BranchId.Value.Value : null,
                connection.CountryCode,
                connection.ProviderCode,
                connection.ExternalAccountReference,
                connection.SecretReference,
                connection.Revision);
    }

    private async Task<RegulatoryIntegrationConnection?> ResolveActiveEntityAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken)
    {
        string normalizedCountry = countryCode.Trim().ToUpperInvariant();
        string normalizedProvider = providerCode.Trim().ToLowerInvariant();

        IQueryable<RegulatoryIntegrationConnection> query = dbContext.RegulatoryIntegrationConnections
            .AsNoTracking()
            .Where(x =>
                x.OrganizationId == organizationId &&
                x.CountryCode == normalizedCountry &&
                x.ProviderCode == normalizedProvider &&
                x.Status == RegulatoryIntegrationConnectionStatus.Active);

        RegulatoryIntegrationConnection? connection = null;

        if (branchId.HasValue)
        {
            connection = await query.FirstOrDefaultAsync(
                x => x.BranchId == branchId,
                cancellationToken);
        }

        connection ??= await query.FirstOrDefaultAsync(
            x => x.BranchId == null,
            cancellationToken);

        return connection;
    }
}
