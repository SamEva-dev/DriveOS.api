namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct LeavePolicyId(Guid Value)
{
    public static LeavePolicyId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
