using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Rules;

public sealed class LegalProfileActivationRule(
    IOrganizationActivationReadinessDataSource dataSource)
    : IOrganizationActivationReadinessRule
{
    public int Order => 10;

    public async Task<OrganizationActivationRequirementResult> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default)
    {
        bool exists = await dataSource.HasActiveLegalProfileAsync(organizationId, cancellationToken);

        return exists
            ? OrganizationActivationRequirementResult.Satisfied(
                OrganizationActivationRequirementCode.LegalProfile,
                "organizations.activationReadiness.requirements.legalProfile.satisfied")
            : OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.LegalProfile,
                "organizations.activationReadiness.requirements.legalProfile.missing");
    }
}
