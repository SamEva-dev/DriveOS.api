namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingCancellationId(Guid Value)
{
    public static BookingCancellationId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
