using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

public static class OrganizationSubscriptionAccessErrors
{
    public static readonly Error SubscriptionUnavailable = Error.Forbidden(
        "OrganizationSubscriptions.Access.SubscriptionUnavailable",
        "errors.organizationSubscription.access.subscriptionUnavailable");

    public static Error EntitlementNotIncluded(string entitlementCode) => Error.Forbidden(
        "OrganizationSubscriptions.Access.EntitlementNotIncluded",
        "errors.organizationSubscription.entitlement.notIncluded",
        new Dictionary<string, object?> { ["entitlementCode"] = entitlementCode });

    public static Error LimitNotAllowed(string limitCode) => Error.Forbidden(
        "OrganizationSubscriptions.Access.LimitNotAllowed",
        "errors.organizationSubscription.limit.notAllowed",
        new Dictionary<string, object?> { ["limitCode"] = limitCode });

    public static Error LimitExceeded(
        string limitCode,
        long limit,
        long currentUsage,
        long requestedIncrease) => Error.Conflict(
        "OrganizationSubscriptions.Access.LimitExceeded",
        "errors.organizationSubscription.limit.exceeded",
        new Dictionary<string, object?>
        {
            ["limitCode"] = limitCode,
            ["limit"] = limit,
            ["currentUsage"] = currentUsage,
            ["requestedIncrease"] = requestedIncrease
        });
}
