namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AvailabilityPlanId(Guid Value)
{
    public static AvailabilityPlanId New() => new(Guid.NewGuid());
    public static AvailabilityPlanId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
