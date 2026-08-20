namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingInstructorReplacementId(Guid Value)
{
    public static BookingInstructorReplacementId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
