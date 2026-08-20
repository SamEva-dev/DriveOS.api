namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct RecurrenceResourceId(Guid Value)
{
    public static RecurrenceResourceId New() => new(Guid.NewGuid());
    public static RecurrenceResourceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
