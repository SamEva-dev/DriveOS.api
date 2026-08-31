namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalInvoiceId(Guid Value)
{
    public static ProfessionalInvoiceId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
