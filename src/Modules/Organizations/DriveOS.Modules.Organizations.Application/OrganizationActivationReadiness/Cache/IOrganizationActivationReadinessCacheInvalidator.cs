using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationActivationReadiness.Cache;

/// <summary>
/// Invalidates the display cache used by the organization activation-readiness checklist.
/// Business decisions must always call IOrganizationActivationReadinessService directly.
/// </summary>
public interface IOrganizationActivationReadinessCacheInvalidator
{
    void Invalidate(OrganizationId organizationId);
}
