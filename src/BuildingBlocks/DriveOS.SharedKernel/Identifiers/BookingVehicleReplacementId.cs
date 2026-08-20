namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BookingVehicleReplacementId(Guid Value)
{
    public static BookingVehicleReplacementId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
