namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct RecurrenceSeriesId(Guid Value)
{
    public static RecurrenceSeriesId New() => new(Guid.NewGuid());
    public static RecurrenceSeriesId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
