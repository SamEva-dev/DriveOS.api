namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingPathId(Guid Value)
{
    public static TrainingPathId New() => new(Guid.NewGuid());
    public static TrainingPathId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
