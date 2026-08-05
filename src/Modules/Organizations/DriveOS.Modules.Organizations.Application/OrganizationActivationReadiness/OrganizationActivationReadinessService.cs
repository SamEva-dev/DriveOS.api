using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;

public sealed class OrganizationActivationReadinessService(
    IEnumerable<IOrganizationActivationReadinessRule> rules)
    : IOrganizationActivationReadinessService
{
    private readonly IReadOnlyCollection<IOrganizationActivationReadinessRule> _rules =
        rules.OrderBy(x => x.Order).ToArray();

    public async Task<OrganizationActivationReadinessReport> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        List<OrganizationActivationRequirementResult> results = [];

        foreach (IOrganizationActivationReadinessRule rule in _rules)
        {
            results.Add(await rule.EvaluateAsync(organizationId, cancellationToken));
        }

        bool isReady = results.All(x =>
            x.IsSatisfied || x.Severity != OrganizationActivationRequirementSeverity.Blocking);

        return new OrganizationActivationReadinessReport(organizationId, isReady, results);
    }
}
