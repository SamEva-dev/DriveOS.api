namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingRescheduleId(Guid Value)
{
    public static BookingRescheduleId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
