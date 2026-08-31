namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalComplianceWaiverId(Guid Value)
{
    public static ProfessionalComplianceWaiverId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
