namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamFailureAnalysisId(Guid Value)
{
    public static ExamFailureAnalysisId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
