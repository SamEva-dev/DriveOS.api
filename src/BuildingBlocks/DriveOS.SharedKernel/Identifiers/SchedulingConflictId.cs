namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct SchedulingConflictId(Guid Value)
{
    public static SchedulingConflictId New() => new(Guid.NewGuid());
    public static SchedulingConflictId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
