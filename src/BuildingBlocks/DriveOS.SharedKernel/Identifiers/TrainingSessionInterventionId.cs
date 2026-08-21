namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionInterventionId(Guid Value)
{
    public static TrainingSessionInterventionId New() => new(Guid.NewGuid());
    public static TrainingSessionInterventionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
