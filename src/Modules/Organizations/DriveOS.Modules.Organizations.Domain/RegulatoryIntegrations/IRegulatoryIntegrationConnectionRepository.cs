using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;

public interface IRegulatoryIntegrationConnectionRepository
{
    Task<RegulatoryIntegrationConnection?> GetForUpdateAsync(OrganizationId organizationId, RegulatoryIntegrationConnectionId id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(OrganizationId organizationId, BranchId? branchId, string countryCode, string providerCode, CancellationToken cancellationToken);
    Task AddAsync(RegulatoryIntegrationConnection connection, CancellationToken cancellationToken);
}
