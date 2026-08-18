namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct PaymentAllocationId(Guid Value)
{
    public static PaymentAllocationId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
