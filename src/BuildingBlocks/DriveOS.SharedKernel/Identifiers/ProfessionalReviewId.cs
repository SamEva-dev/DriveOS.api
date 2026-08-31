namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalReviewId(Guid Value)
{
    public static ProfessionalReviewId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
