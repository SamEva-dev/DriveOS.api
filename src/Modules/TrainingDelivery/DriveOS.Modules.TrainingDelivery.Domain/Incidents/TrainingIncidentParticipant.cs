using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents;

public sealed class TrainingIncidentParticipant : Entity<TrainingIncidentParticipantId>
{
    private TrainingIncidentParticipant() { }
    internal TrainingIncidentParticipant(TrainingIncidentParticipantId id, TrainingIncidentId incidentId, TrainingIncidentParticipantType type, Guid? referenceId, string? label) : base(id)
    {
        TrainingIncidentId = incidentId; Type = type; ReferenceId = referenceId; Label = Normalize(label, 200);
    }
    public TrainingIncidentId TrainingIncidentId { get; private set; }
    public TrainingIncidentParticipantType Type { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string? Label { get; private set; }
    private static string? Normalize(string? value, int max) { var x=value?.Trim(); return string.IsNullOrEmpty(x)?null:(x.Length<=max?x:x[..max]); }
}
