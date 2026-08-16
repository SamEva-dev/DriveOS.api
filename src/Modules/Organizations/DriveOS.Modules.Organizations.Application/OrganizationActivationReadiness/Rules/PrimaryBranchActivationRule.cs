using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationActivationReadiness;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Rules;

public sealed class PrimaryBranchActivationRule(
    IOrganizationActivationReadinessDataSource dataSource
) : IOrganizationActivationReadinessRule
{
    public int Order => 50;

    public async Task<OrganizationActivationRequirementResult> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    )
    {
        BranchId? branchId = await dataSource.GetPrimaryBranchIdAsync(
            organizationId,
            cancellationToken
        );
        if (branchId is null)
        {
            return OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.PrimaryBranch,
                "organizations.activationReadiness.requirements.primaryBranch.missing"
            );
        }

        bool hasManager = await dataSource.HasActiveBranchManagerAsync(
            organizationId,
            branchId.Value,
            cancellationToken
        );

        return hasManager
            ? OrganizationActivationRequirementResult.Satisfied(
                OrganizationActivationRequirementCode.PrimaryBranchManager,
                "organizations.activationReadiness.requirements.primaryBranchManager.satisfied"
            )
            : OrganizationActivationRequirementResult.Missing(
                OrganizationActivationRequirementCode.PrimaryBranchManager,
                "organizations.activationReadiness.requirements.primaryBranchManager.missing"
            );
    }
}
