namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public enum SubscriptionStatus
{
    Trialing = 1,
    Active = 2,
    PastDue = 3,
    Restricted = 4,
    Suspended = 5,
    Cancelled = 6,
    Expired = 7,
}
