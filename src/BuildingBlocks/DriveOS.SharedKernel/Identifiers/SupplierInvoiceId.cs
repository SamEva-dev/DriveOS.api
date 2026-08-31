namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct SupplierInvoiceId(Guid Value)
{
    public static SupplierInvoiceId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
