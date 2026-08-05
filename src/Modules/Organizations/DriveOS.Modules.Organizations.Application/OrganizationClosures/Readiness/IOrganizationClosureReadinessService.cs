using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;

public interface IOrganizationClosureReadinessService
{
    Task<OrganizationClosureReadinessReport> EvaluateAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}

public interface IOrganizationClosureReadinessSnapshotSource
{
    Task<OrganizationClosureReadinessSnapshot> GetAsync(OrganizationId organizationId, CancellationToken cancellationToken);
}

public sealed record OrganizationClosureReadinessSnapshot(
    bool OrganizationExists,
    int ActiveBranches,
    int ActivePrivilegedMemberships,
    bool HasActiveSubscription,
    int OpenOperations,
    int BlockingFinancialItems,
    int ActiveIntegrations,
    bool RetentionPolicyConfigured);
