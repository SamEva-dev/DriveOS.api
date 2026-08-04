namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Access;

public enum OrganizationEntitlementAvailability
{
    Available = 1,
    NotIncluded = 2,
    SubscriptionUnavailable = 3
}

public sealed record OrganizationEntitlementCheckResult(
    OrganizationEntitlementAvailability Availability,
    string EntitlementCode)
{
    public bool IsAvailable => Availability == OrganizationEntitlementAvailability.Available;
}

public enum OrganizationLimitAvailability
{
    Unlimited = 1,
    Available = 2,
    Exceeded = 3,
    NotAllowed = 4,
    SubscriptionUnavailable = 5
}

public sealed record OrganizationLimitCheckResult(
    OrganizationLimitAvailability Availability,
    string LimitCode,
    long? Limit,
    long CurrentUsage,
    long RequestedIncrease)
{
    public bool HasCapacity =>
        Availability is OrganizationLimitAvailability.Unlimited or OrganizationLimitAvailability.Available;
}
