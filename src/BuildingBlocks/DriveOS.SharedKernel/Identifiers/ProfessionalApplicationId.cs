namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalApplicationId(Guid Value)
{
    public static ProfessionalApplicationId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
