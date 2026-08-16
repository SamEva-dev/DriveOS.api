using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

public interface IOrganizationEntitlementChecker
{
    Task<Result<OrganizationEntitlementCheckResult>> CheckAsync(
        OrganizationId organizationId,
        string entitlementCode,
        CancellationToken cancellationToken = default
    );

    Task<Result> RequireAsync(
        OrganizationId organizationId,
        string entitlementCode,
        CancellationToken cancellationToken = default
    );
}
