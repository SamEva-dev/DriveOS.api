using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

/// <summary>
/// Lightweight, append-only field marker captured during a real training session.
/// A marker is contextual evidence for the later report and never constitutes a formal competency assessment by itself.
/// </summary>
public sealed class SessionMarker : Entity<TrainingSessionMarkerId>
{
    private SessionMarker() { }
    private SessionMarker(TrainingSessionMarkerId id) : base(id) { }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public TrainingSessionMarkerType Type { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public CompetencyId? CompetencyId { get; private set; }
    public string ShortNote { get; private set; } = string.Empty;
    public TrainingSessionMarkerSeverity Severity { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public bool CreatedOffline { get; private set; }
    public UserId RecordedByUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }

    internal static Result<SessionMarker> Create(
        TrainingSessionMarkerId id,
        TrainingSessionId sessionId,
        Guid operationId,
        string fingerprint,
        TrainingSessionMarkerType type,
        DateTimeOffset occurredAtUtc,
        CompetencyId? competencyId,
        string shortNote,
        TrainingSessionMarkerSeverity severity,
        decimal? latitude,
        decimal? longitude,
        bool createdOffline,
        UserId actor,
        DateTimeOffset recordedAtUtc)
    {
        if (id.IsEmpty || sessionId.IsEmpty || operationId == Guid.Empty || actor.IsEmpty || !Enum.IsDefined(type) || !Enum.IsDefined(severity))
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerInvalid);
        if (string.IsNullOrWhiteSpace(shortNote))
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerNoteRequired);
        if (shortNote.Trim().Length > 500)
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerNoteTooLong);
        if (latitude.HasValue != longitude.HasValue)
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerLocationInvalid);
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerLocationInvalid);

        return Result.Success(new SessionMarker(id)
        {
            TrainingSessionId = sessionId,
            OperationId = operationId,
            RequestFingerprint = fingerprint,
            Type = type,
            OccurredAtUtc = occurredAtUtc.ToUniversalTime(),
            CompetencyId = competencyId,
            ShortNote = shortNote.Trim(),
            Severity = severity,
            Latitude = latitude,
            Longitude = longitude,
            CreatedOffline = createdOffline,
            RecordedByUserId = actor,
            RecordedAtUtc = recordedAtUtc.ToUniversalTime()
        });
    }
}
