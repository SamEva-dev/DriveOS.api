using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.GetOrganizationSubscription;
public sealed class GetOrganizationSubscriptionQueryHandler(IOrganizationSubscriptionReadService readService) : IQueryHandler<GetOrganizationSubscriptionQuery, OrganizationSubscriptionResponse>
{
    public async Task<Result<OrganizationSubscriptionResponse>> Handle(GetOrganizationSubscriptionQuery query, CancellationToken cancellationToken)
    {
        var response = await readService.GetByOrganizationIdAsync(query.OrganizationId, cancellationToken);
        return response is null ? Result.Failure<OrganizationSubscriptionResponse>(OrganizationSubscriptionErrors.NotFound) : Result.Success(response);
    }
}
