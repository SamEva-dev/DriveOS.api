namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalServiceContractId(Guid Value)
{
    public static ProfessionalServiceContractId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
