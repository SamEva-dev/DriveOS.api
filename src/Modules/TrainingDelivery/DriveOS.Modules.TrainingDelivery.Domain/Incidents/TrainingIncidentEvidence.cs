using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents;

public sealed class TrainingIncidentEvidence : Entity<TrainingIncidentEvidenceId>
{
    private TrainingIncidentEvidence() { }
    internal TrainingIncidentEvidence(TrainingIncidentEvidenceId id, TrainingIncidentId incidentId, Guid documentId, string evidenceType, string? description, UserId addedBy, DateTimeOffset addedAtUtc) : base(id)
    {
        TrainingIncidentId=incidentId; DocumentId=documentId; EvidenceType=evidenceType.Trim(); Description=description?.Trim(); AddedByUserId=addedBy; AddedAtUtc=addedAtUtc.ToUniversalTime();
    }
    public TrainingIncidentId TrainingIncidentId { get; private set; }
    public Guid DocumentId { get; private set; }
    public string EvidenceType { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public UserId AddedByUserId { get; private set; }
    public DateTimeOffset AddedAtUtc { get; private set; }
}
