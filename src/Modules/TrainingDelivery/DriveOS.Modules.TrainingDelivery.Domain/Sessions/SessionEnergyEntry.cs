using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

/// <summary>
/// Immutable vehicle-energy observation captured during one executed training session.
/// Training Delivery owns the observation made in the lesson context; Fleet remains authoritative for the vehicle's global energy and refuelling history.
/// </summary>
public sealed class SessionEnergyEntry : Entity<TrainingSessionEnergyEntryId>
{
    private SessionEnergyEntry() { }
    private SessionEnergyEntry(TrainingSessionEnergyEntryId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public TrainingSessionEnergyEntryType Type { get; private set; }
    public decimal? EnergyLevelPercent { get; private set; }
    public decimal? Quantity { get; private set; }
    public DateTimeOffset ObservedAtUtc { get; private set; }
    public string? Note { get; private set; }
    public bool CreatedOffline { get; private set; }
    public UserId RecordedByUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<SessionEnergyEntry> Create(
        TrainingSessionEnergyEntryId id, TrainingSessionId sessionId, Guid operationId, string fingerprint,
        TrainingSessionEnergyEntryType type, decimal? energyLevelPercent, decimal? quantity, DateTimeOffset observedAtUtc,
        string? note, bool createdOffline, UserId actor, DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || !Enum.IsDefined(type))
            return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyInvalid);
        if (energyLevelPercent is < 0 or > 100)
            return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyLevelInvalid);
        if (type == TrainingSessionEnergyEntryType.LevelObservation && !energyLevelPercent.HasValue)
            return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyLevelRequired);
        if (type is TrainingSessionEnergyEntryType.FuelAdded or TrainingSessionEnergyEntryType.Charging && quantity is not > 0)
            return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyQuantityInvalid);
        if (quantity is > 10_000m || (note?.Length ?? 0) > 500)
            return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyInvalid);

        return Result.Success(new SessionEnergyEntry(id)
        {
            TrainingSessionId = sessionId, OperationId = operationId, RequestFingerprint = fingerprint, Type = type,
            EnergyLevelPercent = energyLevelPercent.HasValue ? decimal.Round(energyLevelPercent.Value, 1, MidpointRounding.AwayFromZero) : null,
            Quantity = quantity.HasValue ? decimal.Round(quantity.Value, 2, MidpointRounding.AwayFromZero) : null,
            ObservedAtUtc = observedAtUtc.ToUniversalTime(), Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(), CreatedOffline = createdOffline,
            RecordedByUserId = actor, RecordedAtUtc = recordedAtUtc.ToUniversalTime()
        });
    }
}
