using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.RegulatoryIntegrations;

public interface IRegulatoryIntegrationConnectionReadService
{
    Task<IReadOnlyList<RegulatoryIntegrationConnectionResponse>> GetAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken);

    Task<RegulatoryIntegrationConnectionResponse?> ResolveActiveAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken);

    /// <summary>
    /// Internal-only transport view. Unlike the public response, this carries SecretReference
    /// so an adapter can resolve secret material from the configured secret store.
    /// The secret value itself is never returned here.
    /// </summary>
    Task<RegulatoryIntegrationTransportConnectionSnapshot?> ResolveActiveTransportAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        string countryCode,
        string providerCode,
        CancellationToken cancellationToken);
}
