namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalReviewReportId(Guid Value)
{
    public static ProfessionalReviewReportId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
