namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionCompetencyAssessmentId(Guid Value)
{
    public static TrainingSessionCompetencyAssessmentId New() => new(Guid.NewGuid());
    public static TrainingSessionCompetencyAssessmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
