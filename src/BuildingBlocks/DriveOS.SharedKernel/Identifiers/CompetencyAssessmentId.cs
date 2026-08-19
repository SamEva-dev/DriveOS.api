namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CompetencyAssessmentId(Guid Value)
{
    public static CompetencyAssessmentId New() => new(Guid.NewGuid());
    public static CompetencyAssessmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
