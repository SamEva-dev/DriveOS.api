using DriveOS.Modules.FundingBilling.Domain.TrainingCredits.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Domain.TrainingCredits;

public sealed class TrainingCreditAccount : AggregateRoot<TrainingCreditAccountId>, IAuditableEntity
{
    private readonly List<TrainingCreditMovement> _movements = [];
    private TrainingCreditAccount() { }

    private TrainingCreditAccount(
        TrainingCreditAccountId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string creditType,
        DateOnly? expirationDate)
        : base(id)
    {
        OrganizationId = organizationId;
        BillingAccountId = billingAccountId;
        CreditType = creditType;
        ExpirationDate = expirationDate;
        Status = TrainingCreditAccountStatus.Active;
    }

    public OrganizationId OrganizationId { get; private set; }
    public BillingAccountId BillingAccountId { get; private set; }
    public string CreditType { get; private set; } = string.Empty;
    public decimal QuantityPurchased { get; private set; }
    public decimal QuantityReserved { get; private set; }
    public decimal QuantityConsumed { get; private set; }
    public decimal Adjustments { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public TrainingCreditAccountStatus Status { get; private set; }
    public IReadOnlyCollection<TrainingCreditMovement> Movements => _movements.AsReadOnly();

    public decimal QuantityAvailable => decimal.Round(
        QuantityPurchased - QuantityReserved - QuantityConsumed + Adjustments,
        2,
        MidpointRounding.AwayFromZero);

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<TrainingCreditAccount> Create(
        TrainingCreditAccountId id,
        OrganizationId organizationId,
        BillingAccountId billingAccountId,
        string creditType,
        DateOnly? expirationDate,
        DateOnly businessDate)
    {
        if (id.IsEmpty)
            return Result.Failure<TrainingCreditAccount>(TrainingCreditAccountErrors.InvalidIdentifier);

        if (organizationId.IsEmpty || billingAccountId.IsEmpty)
            return Result.Failure<TrainingCreditAccount>(TrainingCreditAccountErrors.InvalidOwner);

        string normalizedCreditType = NormalizeCreditType(creditType);
        if (normalizedCreditType.Length is < 2 or > 80 || !normalizedCreditType.All(IsCreditTypeCharacter))
            return Result.Failure<TrainingCreditAccount>(TrainingCreditAccountErrors.InvalidCreditType);

        if (businessDate == default || expirationDate.HasValue && expirationDate.Value < businessDate)
            return Result.Failure<TrainingCreditAccount>(TrainingCreditAccountErrors.InvalidExpirationDate);

        var account = new TrainingCreditAccount(id, organizationId, billingAccountId, normalizedCreditType, expirationDate);
        account.RaiseDomainEvent(new TrainingCreditAccountCreatedDomainEvent(account.Id, account.BillingAccountId, account.CreditType, account.ExpirationDate));
        return Result.Success(account);
    }

    public bool IsExpiredOn(DateOnly businessDate) => ExpirationDate.HasValue && businessDate > ExpirationDate.Value;

    public Result<TrainingCreditMovementId> Purchase(TrainingCreditMovementId movementId, decimal quantity, string reference, string? reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        Result check = CanOperate(quantity, actorUserId, occurredAtUtc);
        if (check.IsFailure) return Result.Failure<TrainingCreditMovementId>(check.Error);
        Result<TrainingCreditMovement> movement = TrainingCreditMovement.Create(movementId, Id, TrainingCreditMovementType.Purchase, quantity, reference, reason, occurredAtUtc, actorUserId);
        if (movement.IsFailure) return Result.Failure<TrainingCreditMovementId>(movement.Error);
        QuantityPurchased = Round(QuantityPurchased + quantity);
        return Record(movement.Value);
    }

    public Result<TrainingCreditMovementId> Reserve(TrainingCreditMovementId movementId, decimal quantity, string reference, string? reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        Result check = CanOperate(quantity, actorUserId, occurredAtUtc);
        if (check.IsFailure) return Result.Failure<TrainingCreditMovementId>(check.Error);
        if (Round(quantity) > QuantityAvailable) return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.InsufficientAvailable);
        Result<TrainingCreditMovement> movement = TrainingCreditMovement.Create(movementId, Id, TrainingCreditMovementType.Reservation, quantity, reference, reason, occurredAtUtc, actorUserId);
        if (movement.IsFailure) return Result.Failure<TrainingCreditMovementId>(movement.Error);
        QuantityReserved = Round(QuantityReserved + quantity);
        return Record(movement.Value);
    }

    public Result<TrainingCreditMovementId> Release(TrainingCreditMovementId movementId, decimal quantity, string reference, string? reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        Result check = CanOperate(quantity, actorUserId, occurredAtUtc);
        if (check.IsFailure) return Result.Failure<TrainingCreditMovementId>(check.Error);
        if (Round(quantity) > QuantityReserved) return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.InsufficientReserved);
        Result<TrainingCreditMovement> movement = TrainingCreditMovement.Create(movementId, Id, TrainingCreditMovementType.Release, quantity, reference, reason, occurredAtUtc, actorUserId);
        if (movement.IsFailure) return Result.Failure<TrainingCreditMovementId>(movement.Error);
        QuantityReserved = Round(QuantityReserved - quantity);
        return Record(movement.Value);
    }

    public Result<TrainingCreditMovementId> Consume(TrainingCreditMovementId movementId, decimal quantity, string reference, string? reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        Result check = CanOperate(quantity, actorUserId, occurredAtUtc);
        if (check.IsFailure) return Result.Failure<TrainingCreditMovementId>(check.Error);
        if (Round(quantity) > QuantityReserved) return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.InsufficientReserved);
        Result<TrainingCreditMovement> movement = TrainingCreditMovement.Create(movementId, Id, TrainingCreditMovementType.Consumption, quantity, reference, reason, occurredAtUtc, actorUserId);
        if (movement.IsFailure) return Result.Failure<TrainingCreditMovementId>(movement.Error);
        QuantityReserved = Round(QuantityReserved - quantity);
        QuantityConsumed = Round(QuantityConsumed + quantity);
        return Record(movement.Value);
    }

    public Result<TrainingCreditMovementId> Adjust(TrainingCreditMovementId movementId, decimal quantity, string reference, string reason, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (quantity == 0m) return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.MovementInvalid);
        Result check = CanOperate(Math.Abs(quantity), actorUserId, occurredAtUtc);
        if (check.IsFailure) return Result.Failure<TrainingCreditMovementId>(check.Error);
        decimal nextAdjustments = Round(Adjustments + quantity);
        decimal nextAvailable = Round(QuantityPurchased - QuantityReserved - QuantityConsumed + nextAdjustments);
        if (nextAvailable < 0m) return Result.Failure<TrainingCreditMovementId>(TrainingCreditAccountErrors.AdjustmentWouldOverdraw);
        Result<TrainingCreditMovement> movement = TrainingCreditMovement.Create(movementId, Id, TrainingCreditMovementType.Adjustment, quantity, reference, reason, occurredAtUtc, actorUserId);
        if (movement.IsFailure) return Result.Failure<TrainingCreditMovementId>(movement.Error);
        Adjustments = nextAdjustments;
        return Record(movement.Value);
    }

    private Result CanOperate(decimal quantity, UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != TrainingCreditAccountStatus.Active || IsExpiredOn(DateOnly.FromDateTime(occurredAtUtc.UtcDateTime))) return Result.Failure(TrainingCreditAccountErrors.OperationNotAllowed);
        if (quantity <= 0m || actorUserId.IsEmpty || occurredAtUtc == default) return Result.Failure(TrainingCreditAccountErrors.MovementInvalid);
        return Result.Success();
    }

    private Result<TrainingCreditMovementId> Record(TrainingCreditMovement movement)
    {
        _movements.Add(movement);
        RaiseDomainEvent(new TrainingCreditMovementRecordedDomainEvent(Id, movement.Id, movement.Type, movement.Quantity, movement.Reference, movement.ActorUserId, movement.OccurredAtUtc));
        return Result.Success(movement.Id);
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default) return;
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private static string NormalizeCreditType(string creditType) => creditType?.Trim().ToUpperInvariant() ?? string.Empty;
    private static bool IsCreditTypeCharacter(char character) => char.IsLetterOrDigit(character) || character is '_' or '-' or '.';
}
