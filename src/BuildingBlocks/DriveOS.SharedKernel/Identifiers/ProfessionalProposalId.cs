namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalProposalId(Guid Value)
{
    public static ProfessionalProposalId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
