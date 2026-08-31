namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalCommercialOfferId(Guid Value)
{
    public static ProfessionalCommercialOfferId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
