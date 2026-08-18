namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BillingPartyId(Guid Value)
{
    public static BillingPartyId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
