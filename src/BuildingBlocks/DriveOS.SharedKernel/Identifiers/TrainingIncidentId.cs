namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingIncidentId(Guid Value)
{
    public static TrainingIncidentId New() => new(Guid.NewGuid());
    public static TrainingIncidentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
