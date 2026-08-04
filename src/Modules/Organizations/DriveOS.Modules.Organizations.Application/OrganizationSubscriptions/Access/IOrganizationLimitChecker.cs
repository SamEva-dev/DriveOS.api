using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

public interface IOrganizationLimitChecker
{
    Task<Result<OrganizationLimitCheckResult>> CheckAsync(
        OrganizationId organizationId,
        string limitCode,
        long currentUsage,
        long requestedIncrease,
        CancellationToken cancellationToken = default);

    Task<Result> RequireCapacityAsync(
        OrganizationId organizationId,
        string limitCode,
        long currentUsage,
        long requestedIncrease,
        CancellationToken cancellationToken = default);
}
