namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AvailabilityExceptionId(Guid Value)
{
    public static AvailabilityExceptionId New() => new(Guid.NewGuid());
    public static AvailabilityExceptionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
