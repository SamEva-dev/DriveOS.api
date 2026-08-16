using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness;

public interface IOrganizationActivationReadinessService
{
    Task<OrganizationActivationReadinessReport> EvaluateAsync(
        OrganizationId organizationId,
        CancellationToken cancellationToken = default
    );
}
