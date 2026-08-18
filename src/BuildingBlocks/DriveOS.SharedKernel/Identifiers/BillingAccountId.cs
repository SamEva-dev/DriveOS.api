namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BillingAccountId(Guid Value)
{
    public static BillingAccountId New() => new(Guid.NewGuid());

    public static BillingAccountId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
