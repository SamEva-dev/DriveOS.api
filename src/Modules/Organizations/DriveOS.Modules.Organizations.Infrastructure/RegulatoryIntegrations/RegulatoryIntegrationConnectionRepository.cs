using DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.Organizations.Infrastructure.RegulatoryIntegrations;

internal sealed class RegulatoryIntegrationConnectionRepository(OrganizationsDbContext dbContext) : IRegulatoryIntegrationConnectionRepository
{
    public Task<RegulatoryIntegrationConnection?> GetForUpdateAsync(OrganizationId organizationId, RegulatoryIntegrationConnectionId id, CancellationToken cancellationToken)
        => dbContext.RegulatoryIntegrationConnections.FirstOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(OrganizationId organizationId, BranchId? branchId, string countryCode, string providerCode, CancellationToken cancellationToken)
        => dbContext.RegulatoryIntegrationConnections.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.BranchId == branchId && x.CountryCode == countryCode && x.ProviderCode == providerCode, cancellationToken);

    public Task AddAsync(RegulatoryIntegrationConnection connection, CancellationToken cancellationToken)
        => dbContext.RegulatoryIntegrationConnections.AddAsync(connection, cancellationToken).AsTask();
}
