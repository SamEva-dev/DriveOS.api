namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct SupplierPaymentAttemptId(Guid Value)
{
    public static SupplierPaymentAttemptId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
