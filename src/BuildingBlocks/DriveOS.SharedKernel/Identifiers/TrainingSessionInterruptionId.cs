namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionInterruptionId(Guid Value)
{
    public static TrainingSessionInterruptionId New() => new(Guid.NewGuid());
    public static TrainingSessionInterruptionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
