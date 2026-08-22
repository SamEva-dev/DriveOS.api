namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an exam resource assignment aggregate.</summary>
public readonly record struct ExamResourceAssignmentId(Guid Value)
{
    public static ExamResourceAssignmentId New() => new(Guid.NewGuid());
    public static ExamResourceAssignmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
