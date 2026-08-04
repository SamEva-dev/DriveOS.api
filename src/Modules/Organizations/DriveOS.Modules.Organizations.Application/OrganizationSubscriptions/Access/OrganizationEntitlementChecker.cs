using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

public sealed class OrganizationEntitlementChecker(
    IOrganizationSubscriptionReadService readService) : IOrganizationEntitlementChecker
{
    public async Task<Result<OrganizationEntitlementCheckResult>> CheckAsync(
        OrganizationId organizationId,
        string entitlementCode,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entitlementCode);

        var subscription = await readService.GetByOrganizationIdAsync(
            organizationId,
            cancellationToken);

        if (subscription is null)
        {
            return Result.Failure<OrganizationEntitlementCheckResult>(
                OrganizationSubscriptionErrors.NotFound);
        }

        string normalizedCode = entitlementCode.Trim();

        if (!IsOperationalStatus((SubscriptionStatus)subscription.Status))
        {
            return Result.Success(
                new OrganizationEntitlementCheckResult(
                    OrganizationEntitlementAvailability.SubscriptionUnavailable,
                    normalizedCode));
        }

        bool included = subscription.Entitlements.Any(
            entitlement => string.Equals(
                entitlement.Code,
                normalizedCode,
                StringComparison.Ordinal));

        return Result.Success(
            new OrganizationEntitlementCheckResult(
                included
                    ? OrganizationEntitlementAvailability.Available
                    : OrganizationEntitlementAvailability.NotIncluded,
                normalizedCode));
    }

    public async Task<Result> RequireAsync(
        OrganizationId organizationId,
        string entitlementCode,
        CancellationToken cancellationToken = default)
    {
        Result<OrganizationEntitlementCheckResult> check = await CheckAsync(
            organizationId,
            entitlementCode,
            cancellationToken);

        if (check.IsFailure)
        {
            return Result.Failure(check.Error);
        }

        return check.Value.Availability switch
        {
            OrganizationEntitlementAvailability.Available => Result.Success(),
            OrganizationEntitlementAvailability.NotIncluded => Result.Failure(
                OrganizationSubscriptionAccessErrors.EntitlementNotIncluded(check.Value.EntitlementCode)),
            _ => Result.Failure(OrganizationSubscriptionAccessErrors.SubscriptionUnavailable)
        };
    }

    private static bool IsOperationalStatus(SubscriptionStatus status) =>
        status is SubscriptionStatus.Trialing
            or SubscriptionStatus.Active
            or SubscriptionStatus.PastDue
            or SubscriptionStatus.Restricted;
}
