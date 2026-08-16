using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Rules;

public sealed class OperationalSettingsActivationRule(
    IOrganizationActivationReadinessDataSource dataSource
) : IOrganizationActivationReadinessRule
{
    public int Order => 40;

    public async Task<OrganizationActivationRequirementResult> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        bool exists = await dataSource.HasOperationalSettingsAsync(
            organizationId,
            cancellationToken
        );

        return exists
            ? OrganizationActivationRequirementResult.Satisfied(
                OrganizationActivationRequirementCode.OperationalSettings,
                "organizations.activationReadiness.requirements.operationalSettings.satisfied"
            )
            : OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.OperationalSettings,
                "organizations.activationReadiness.requirements.operationalSettings.missing"
            );
    }
}
