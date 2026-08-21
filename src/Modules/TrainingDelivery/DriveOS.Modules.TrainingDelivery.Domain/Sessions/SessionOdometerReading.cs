using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed class SessionOdometerReading : Entity<TrainingSessionOdometerReadingId>
{
    private SessionOdometerReading() { }
    private SessionOdometerReading(TrainingSessionOdometerReadingId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public decimal OdometerKilometers { get; private set; }
    public TrainingSessionOdometerSource Source { get; private set; }
    public DateTimeOffset ObservedAtUtc { get; private set; }
    public UserId RecordedByUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<SessionOdometerReading> Create(
        TrainingSessionOdometerReadingId id,
        TrainingSessionId sessionId,
        Guid operationId,
        string fingerprint,
        decimal kilometers,
        TrainingSessionOdometerSource source,
        DateTimeOffset observedAtUtc,
        UserId actor,
        DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || !Enum.IsDefined(source) || kilometers < 0 || kilometers > 10_000_000m)
            return Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerInvalid);

        return Result.Success(new SessionOdometerReading(id)
        {
            TrainingSessionId = sessionId,
            OperationId = operationId,
            RequestFingerprint = fingerprint,
            OdometerKilometers = decimal.Round(kilometers, 1, MidpointRounding.AwayFromZero),
            Source = source,
            ObservedAtUtc = observedAtUtc.ToUniversalTime(),
            RecordedByUserId = actor,
            RecordedAtUtc = recordedAtUtc.ToUniversalTime()
        });
    }
}
