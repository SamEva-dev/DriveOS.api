namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingPathMilestoneId(Guid Value)
{
    public static TrainingPathMilestoneId New() => new(Guid.NewGuid());
    public static TrainingPathMilestoneId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
