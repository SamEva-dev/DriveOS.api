namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingIncidentEvidenceId(Guid Value)
{
    public static TrainingIncidentEvidenceId New() => new(Guid.NewGuid());
    public static TrainingIncidentEvidenceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
