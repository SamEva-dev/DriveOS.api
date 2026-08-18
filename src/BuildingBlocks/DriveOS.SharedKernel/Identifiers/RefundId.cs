namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct RefundId(Guid Value)
{
    public static RefundId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
