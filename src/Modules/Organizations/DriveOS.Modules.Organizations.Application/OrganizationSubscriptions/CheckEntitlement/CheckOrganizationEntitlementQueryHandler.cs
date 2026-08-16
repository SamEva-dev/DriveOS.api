using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckEntitlement;

public sealed class CheckOrganizationEntitlementQueryHandler(
    IOrganizationSubscriptionReadService readService
) : IQueryHandler<CheckOrganizationEntitlementQuery, bool>
{
    public async Task<Result<bool>> Handle(
        CheckOrganizationEntitlementQuery query,
        CancellationToken cancellationToken
    ) =>
        Result.Success(
            await readService.HasEntitlementAsync(
                query.OrganizationId,
                query.EntitlementCode,
                cancellationToken
            )
        );
}
