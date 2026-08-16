namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CheckLimit;

public enum OrganizationLimitAvailability
{
    Unlimited = 1,
    Available = 2,
    Exceeded = 3,
    NotAllowed = 4,
}

public sealed record OrganizationLimitCheckResponse(
    OrganizationLimitAvailability Availability,
    long? Limit,
    long CurrentUsage,
    long RequestedIncrease
);
