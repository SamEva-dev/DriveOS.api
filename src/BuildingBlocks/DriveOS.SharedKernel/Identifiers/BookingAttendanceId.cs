namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingAttendanceId(Guid Value)
{
    public static BookingAttendanceId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
