using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed class SessionObservation : Entity<TrainingSessionObservationId>
{
    private SessionObservation() { }
    private SessionObservation(TrainingSessionObservationId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public TrainingSessionObservationType Type { get; private set; }
    public DateTimeOffset ObservedAtUtc { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsInternal { get; private set; }
    public UserId RecordedByUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<SessionObservation> Create(
        TrainingSessionObservationId id,
        TrainingSessionId sessionId,
        Guid operationId,
        string fingerprint,
        TrainingSessionObservationType type,
        DateTimeOffset observedAtUtc,
        string content,
        bool isInternal,
        UserId actor,
        DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || !Enum.IsDefined(type))
            return Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationInvalid);
        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationRequired);
        if (content.Trim().Length > 4000)
            return Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationTooLong);

        return Result.Success(new SessionObservation(id)
        {
            TrainingSessionId = sessionId,
            OperationId = operationId,
            RequestFingerprint = fingerprint,
            Type = type,
            ObservedAtUtc = observedAtUtc.ToUniversalTime(),
            Content = content.Trim(),
            IsInternal = isInternal,
            RecordedByUserId = actor,
            RecordedAtUtc = recordedAtUtc.ToUniversalTime()
        });
    }
}
