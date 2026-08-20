namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct RecurrenceOccurrenceId(Guid Value)
{
    public static RecurrenceOccurrenceId New() => new(Guid.NewGuid());
    public static RecurrenceOccurrenceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
