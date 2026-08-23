namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct WorkingTimePolicyId(Guid Value)
{
    public static WorkingTimePolicyId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
