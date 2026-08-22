namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for a vehicle resource.</summary>
public readonly record struct VehicleId(Guid Value)
{
    public static VehicleId New() => new(Guid.NewGuid());
    public static VehicleId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
