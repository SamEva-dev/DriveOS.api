namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingResourceId(Guid Value)
{
    public static BookingResourceId New() => new(Guid.NewGuid());
    public static BookingResourceId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
