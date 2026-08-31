namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct SupplierPaymentBatchId(Guid Value)
{
    public static SupplierPaymentBatchId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
