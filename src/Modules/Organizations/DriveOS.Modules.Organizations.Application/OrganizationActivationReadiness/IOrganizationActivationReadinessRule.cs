using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;

public interface IOrganizationActivationReadinessRule
{
    int Order { get; }

    Task<OrganizationActivationRequirementResult> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default);
}
