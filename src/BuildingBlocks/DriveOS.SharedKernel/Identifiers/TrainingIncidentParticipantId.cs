namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingIncidentParticipantId(Guid Value)
{
    public static TrainingIncidentParticipantId New() => new(Guid.NewGuid());
    public static TrainingIncidentParticipantId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
