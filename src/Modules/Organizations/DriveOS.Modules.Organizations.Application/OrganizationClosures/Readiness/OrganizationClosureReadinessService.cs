using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationClosures.Readiness;

public sealed class OrganizationClosureReadinessService(IOrganizationClosureReadinessSnapshotSource source)
    : IOrganizationClosureReadinessService
{
    public async Task<OrganizationClosureReadinessReport> EvaluateAsync(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        OrganizationClosureReadinessSnapshot s = await source.GetAsync(organizationId, cancellationToken);
        var items = new List<OrganizationClosureRequirement>
        {
            R("organization.exists", s.OrganizationExists, OrganizationClosureRequirementSeverity.Blocking),
            R("branches.closed-or-planned", s.ActiveBranches == 0, OrganizationClosureRequirementSeverity.Blocking, s.ActiveBranches),
            R("memberships.revocation-planned", s.ActivePrivilegedMemberships == 0, OrganizationClosureRequirementSeverity.Warning, s.ActivePrivilegedMemberships),
            R("subscription.terminated-or-planned", !s.HasActiveSubscription, OrganizationClosureRequirementSeverity.Blocking),
            R("operations.closed", s.OpenOperations == 0, OrganizationClosureRequirementSeverity.Blocking, s.OpenOperations),
            R("finance.settled-or-flagged", s.BlockingFinancialItems == 0, OrganizationClosureRequirementSeverity.Blocking, s.BlockingFinancialItems),
            R("integrations.disabled", s.ActiveIntegrations == 0, OrganizationClosureRequirementSeverity.Blocking, s.ActiveIntegrations),
            R("retention.policy-configured", s.RetentionPolicyConfigured, OrganizationClosureRequirementSeverity.Blocking)
        };
        return new OrganizationClosureReadinessReport(organizationId, items);
    }

    private static OrganizationClosureRequirement R(string code, bool ok, OrganizationClosureRequirementSeverity severity, int? count = null) =>
        new(code, ok, severity, $"organizations.closure.requirements.{code}", count is null ? new Dictionary<string, object?>() : new Dictionary<string, object?> { ["count"] = count });
}
