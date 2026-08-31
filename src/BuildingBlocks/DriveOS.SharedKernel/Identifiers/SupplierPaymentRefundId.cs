namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct SupplierPaymentRefundId(Guid Value)
{
    public static SupplierPaymentRefundId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
