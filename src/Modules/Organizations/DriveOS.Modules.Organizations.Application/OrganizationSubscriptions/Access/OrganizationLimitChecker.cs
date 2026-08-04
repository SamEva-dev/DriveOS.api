using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

public sealed class OrganizationLimitChecker(
    IOrganizationSubscriptionReadService readService) : IOrganizationLimitChecker
{
    public async Task<Result<OrganizationLimitCheckResult>> CheckAsync(
        OrganizationId organizationId,
        string limitCode,
        long currentUsage,
        long requestedIncrease,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(limitCode);
        ArgumentOutOfRangeException.ThrowIfNegative(currentUsage);
        ArgumentOutOfRangeException.ThrowIfNegative(requestedIncrease);

        var subscription = await readService.GetByOrganizationIdAsync(
            organizationId,
            cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<OrganizationLimitCheckResult>(
                OrganizationSubscriptionErrors.NotFound);
        }

        string normalizedCode = limitCode.Trim();

        if (!IsOperationalStatus((SubscriptionStatus)subscription.Status))
        {
            return Result.Success(
                new OrganizationLimitCheckResult(
                    OrganizationLimitAvailability.SubscriptionUnavailable,
                    normalizedCode,
                    null,
                    currentUsage,
                    requestedIncrease));
        }

        long? limit = subscription.Limits
            .Where(item => string.Equals(item.Code, normalizedCode, StringComparison.Ordinal))
            .Select(item => (long?)item.Value)
            .SingleOrDefault();

        OrganizationLimitAvailability availability;
        if (!limit.HasValue)
        {
            availability = OrganizationLimitAvailability.Unlimited;
        }
        else if (limit.Value == 0)
        {
            availability = OrganizationLimitAvailability.NotAllowed;
        }
        else
        {
            long requestedUsage;
            try
            {
                requestedUsage = checked(currentUsage + requestedIncrease);
            }
            catch (OverflowException)
            {
                requestedUsage = long.MaxValue;
            }

            availability = requestedUsage <= limit.Value
                ? OrganizationLimitAvailability.Available
                : OrganizationLimitAvailability.Exceeded;
        }

        return Result.Success(
            new OrganizationLimitCheckResult(
                availability,
                normalizedCode,
                limit,
                currentUsage,
                requestedIncrease));
    }

    public async Task<Result> RequireCapacityAsync(
        OrganizationId organizationId,
        string limitCode,
        long currentUsage,
        long requestedIncrease,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationLimitCheckResult> check = await CheckAsync(
            organizationId,
            limitCode,
            currentUsage,
            requestedIncrease,
            cancellationToken);

        if (check.IsFailure)
        {
            return Result.Failure(check.Error);
        }

        return check.Value.Availability switch
        {
            OrganizationLimitAvailability.Unlimited or
            OrganizationLimitAvailability.Available => Result.Success(),

            OrganizationLimitAvailability.NotAllowed => Result.Failure(
                OrganizationSubscriptionAccessErrors.LimitNotAllowed(check.Value.LimitCode)),

            OrganizationLimitAvailability.Exceeded => Result.Failure(
                OrganizationSubscriptionAccessErrors.LimitExceeded(
                    check.Value.LimitCode,
                    check.Value.Limit!.Value,
                    check.Value.CurrentUsage,
                    check.Value.RequestedIncrease)),

            _ => Result.Failure(OrganizationSubscriptionAccessErrors.SubscriptionUnavailable)
        };
    }

    private static bool IsOperationalStatus(SubscriptionStatus status) =>
        status is SubscriptionStatus.Trialing
            or SubscriptionStatus.Active
            or SubscriptionStatus.PastDue
            or SubscriptionStatus.Restricted;
}
