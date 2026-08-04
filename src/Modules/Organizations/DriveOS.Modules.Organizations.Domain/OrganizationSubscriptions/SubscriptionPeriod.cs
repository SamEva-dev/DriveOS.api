using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public sealed record SubscriptionPeriod
{
    private SubscriptionPeriod(DateTimeOffset startsAtUtc, DateTimeOffset? endsAtUtc)
    {
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset? EndsAtUtc { get; }

    public static Result<SubscriptionPeriod> Create(
        DateTimeOffset startsAtUtc,
        DateTimeOffset? endsAtUtc)
    {
        if (endsAtUtc.HasValue && endsAtUtc.Value <= startsAtUtc)
        {
            return Result.Failure<SubscriptionPeriod>(
                OrganizationSubscriptionErrors.InvalidPeriod);
        }

        return Result.Success(new SubscriptionPeriod(startsAtUtc, endsAtUtc));
    }

    public bool Contains(DateTimeOffset instant) =>
        instant >= StartsAtUtc &&
        (!EndsAtUtc.HasValue || instant < EndsAtUtc.Value);
}
