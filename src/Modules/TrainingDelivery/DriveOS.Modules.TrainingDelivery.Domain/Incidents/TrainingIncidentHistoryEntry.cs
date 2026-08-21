using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents;

public sealed class TrainingIncidentHistoryEntry : Entity<TrainingIncidentHistoryEntryId>
{
    private TrainingIncidentHistoryEntry() { }
    internal TrainingIncidentHistoryEntry(TrainingIncidentHistoryEntryId id, TrainingIncidentId incidentId, Guid operationId, string fingerprint, TrainingIncidentHistoryAction action, TrainingIncidentStatus fromStatus, TrainingIncidentStatus toStatus, string? reason, UserId actor, DateTimeOffset occurredAtUtc) : base(id)
    {
        TrainingIncidentId=incidentId; OperationId=operationId; RequestFingerprint=fingerprint; Action=action; FromStatus=fromStatus; ToStatus=toStatus; Reason=reason?.Trim(); ActorUserId=actor; OccurredAtUtc=occurredAtUtc.ToUniversalTime();
    }
    public TrainingIncidentId TrainingIncidentId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public TrainingIncidentHistoryAction Action { get; private set; }
    public TrainingIncidentStatus FromStatus { get; private set; }
    public TrainingIncidentStatus ToStatus { get; private set; }
    public string? Reason { get; private set; }
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
}
