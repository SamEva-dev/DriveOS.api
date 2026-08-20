namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CalendarResourceId(Guid Value)
{
    public static CalendarResourceId New() => new(Guid.NewGuid());
    public static CalendarResourceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
