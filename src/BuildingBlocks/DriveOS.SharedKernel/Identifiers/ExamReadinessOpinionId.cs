namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for a pedagogical exam-readiness opinion.</summary>
public readonly record struct ExamReadinessOpinionId(Guid Value)
{
    public static ExamReadinessOpinionId New() => new(Guid.NewGuid());
    public static ExamReadinessOpinionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
