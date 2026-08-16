using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public sealed record SubscriptionCancellation
{
    public const int ReasonMaximumLength = 500;

    private SubscriptionCancellation(
        DateTimeOffset requestedAtUtc,
        DateTimeOffset effectiveAtUtc,
        string reason,
        UserId requestedByUserId
    )
    {
        RequestedAtUtc = requestedAtUtc;
        EffectiveAtUtc = effectiveAtUtc;
        Reason = reason;
        RequestedByUserId = requestedByUserId;
    }

    public DateTimeOffset RequestedAtUtc { get; }
    public DateTimeOffset EffectiveAtUtc { get; }
    public string Reason { get; }
    public UserId RequestedByUserId { get; }

    public static Result<SubscriptionCancellation> Create(
        DateTimeOffset requestedAtUtc,
        DateTimeOffset effectiveAtUtc,
        string? reason,
        UserId requestedByUserId
    )
    {
        string normalizedReason = reason?.Trim() ?? string.Empty;

        if (requestedByUserId.IsEmpty)
        {
            return Result.Failure<SubscriptionCancellation>(
                OrganizationSubscriptionErrors.EmptyActorUserId
            );
        }

        if (string.IsNullOrWhiteSpace(normalizedReason))
        {
            return Result.Failure<SubscriptionCancellation>(
                OrganizationSubscriptionErrors.EmptyChangeReason
            );
        }

        if (normalizedReason.Length > ReasonMaximumLength)
        {
            return Result.Failure<SubscriptionCancellation>(
                OrganizationSubscriptionErrors.ChangeReasonTooLong(ReasonMaximumLength)
            );
        }

        if (effectiveAtUtc < requestedAtUtc)
        {
            return Result.Failure<SubscriptionCancellation>(
                OrganizationSubscriptionErrors.InvalidCancellationDate
            );
        }

        return Result.Success(
            new SubscriptionCancellation(
                requestedAtUtc,
                effectiveAtUtc,
                normalizedReason,
                requestedByUserId
            )
        );
    }
}
