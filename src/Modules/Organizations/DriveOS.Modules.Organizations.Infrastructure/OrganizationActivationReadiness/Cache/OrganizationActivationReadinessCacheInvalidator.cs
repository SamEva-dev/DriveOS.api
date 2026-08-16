using DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationActivationReadiness.Cache;

internal sealed class OrganizationActivationReadinessCacheInvalidator(
    IOrganizationActivationReadinessReportCache cache
) : IOrganizationActivationReadinessCacheInvalidator
{
    public void Invalidate(OrganizationId organizationId) => cache.Invalidate(organizationId);
}
