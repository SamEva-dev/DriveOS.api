using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Rules;

public sealed class SubscriptionActivationRule(
    IOrganizationActivationReadinessDataSource dataSource)
    : IOrganizationActivationReadinessRule
{
    public int Order => 30;

    public async Task<OrganizationActivationRequirementResult> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        bool exists = await dataSource.HasActiveSubscriptionAsync(organizationId, cancellationToken);

        return exists
            ? OrganizationActivationRequirementResult.Satisfied(
                OrganizationActivationRequirementCode.ActiveSubscription,
                "organizations.activationReadiness.requirements.subscription.satisfied")
            : OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.ActiveSubscription,
                "organizations.activationReadiness.requirements.subscription.missing");
    }
}
