using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Models;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;

public interface IOrganizationActivationReadinessReportCache
{
    Task<OrganizationActivationReadinessReport> GetOrCreateAsync(
        OrganizationId organizationId,
        Func<CancellationToken, Task<OrganizationActivationReadinessReport>> factory,
        CancellationToken cancellationToken = default
    );

    void Invalidate(OrganizationId organizationId);
}
