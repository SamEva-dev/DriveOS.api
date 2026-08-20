namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AvailabilityRuleId(Guid Value)
{
    public static AvailabilityRuleId New() => new(Guid.NewGuid());
    public static AvailabilityRuleId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
