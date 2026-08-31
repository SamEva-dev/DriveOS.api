namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalEngagementId(Guid Value)
{
    public static ProfessionalEngagementId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
