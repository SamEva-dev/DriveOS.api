using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckLimit;

public sealed class CheckOrganizationLimitQueryHandler(
    IOrganizationSubscriptionReadService readService
) : IQueryHandler<CheckOrganizationLimitQuery, OrganizationLimitCheckResponse>
{
    public async Task<Result<OrganizationLimitCheckResponse>> Handle(
        CheckOrganizationLimitQuery query,
        CancellationToken cancellationToken
    )
    {
        long? limit = await readService.GetLimitAsync(
            query.OrganizationId,
            query.LimitCode,
            cancellationToken
        );
        OrganizationLimitAvailability availability =
            !limit.HasValue ? OrganizationLimitAvailability.Unlimited
            : limit.Value == 0 ? OrganizationLimitAvailability.NotAllowed
            : query.CurrentUsage + query.RequestedIncrease <= limit.Value
                ? OrganizationLimitAvailability.Available
            : OrganizationLimitAvailability.Exceeded;
        return Result.Success(
            new OrganizationLimitCheckResponse(
                availability,
                limit,
                query.CurrentUsage,
                query.RequestedIncrease
            )
        );
    }
}
