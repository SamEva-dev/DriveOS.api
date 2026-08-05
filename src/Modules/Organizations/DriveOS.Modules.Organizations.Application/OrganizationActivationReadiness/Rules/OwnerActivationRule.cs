using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Rules;

public sealed class OwnerActivationRule(
    IOrganizationActivationReadinessDataSource dataSource)
    : IOrganizationActivationReadinessRule
{
    public int Order => 20;

    public async Task<OrganizationActivationRequirementResult> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        bool hasOwner = await dataSource.HasActiveOwnerAsync(organizationId, cancellationToken);
        if (!hasOwner)
        {
            return OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.ActiveOwner,
                "organizations.activationReadiness.requirements.activeOwner.missing");
        }

        bool hasPrimaryOwner = await dataSource.HasActivePrimaryOwnerAsync(organizationId, cancellationToken);
        return hasPrimaryOwner
            ? OrganizationActivationRequirementResult.Satisfied(
                OrganizationActivationRequirementCode.PrimaryOwner,
                "organizations.activationReadiness.requirements.primaryOwner.satisfied")
            : OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.PrimaryOwner,
                "organizations.activationReadiness.requirements.primaryOwner.missing");
    }
}
