using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.TrainingCredits;

public sealed class TrainingCreditMovement
{
    private TrainingCreditMovement() { }

    private TrainingCreditMovement(TrainingCreditMovementId id, TrainingCreditAccountId accountId,
        TrainingCreditMovementType type, decimal quantity, string reference, string? reason,
        DateTimeOffset occurredAtUtc, UserId actorUserId)
    {
        Id = id;
        TrainingCreditAccountId = accountId;
        Type = type;
        Quantity = Round(quantity);
        Reference = reference;
        Reason = reason;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
        ActorUserId = actorUserId;
    }

    public TrainingCreditMovementId Id { get; private set; }
    public TrainingCreditAccountId TrainingCreditAccountId { get; private set; }
    public TrainingCreditMovementType Type { get; private set; }
    public decimal Quantity { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string? Reason { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public UserId ActorUserId { get; private set; }

    internal static Result<TrainingCreditMovement> Create(TrainingCreditMovementId id,
        TrainingCreditAccountId accountId, TrainingCreditMovementType type, decimal quantity,
        string reference, string? reason, DateTimeOffset occurredAtUtc, UserId actorUserId)
    {
        string normalizedReference = reference?.Trim() ?? string.Empty;
        string? normalizedReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (id.IsEmpty || accountId.IsEmpty || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure<TrainingCreditMovement>(TrainingCreditAccountErrors.MovementInvalid);
        if (quantity == 0m || normalizedReference.Length is < 3 or > 200 || normalizedReason?.Length > 1000)
            return Result.Failure<TrainingCreditMovement>(TrainingCreditAccountErrors.MovementInvalid);
        return Result.Success(new TrainingCreditMovement(id, accountId, type, quantity, normalizedReference, normalizedReason, occurredAtUtc, actorUserId));
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
