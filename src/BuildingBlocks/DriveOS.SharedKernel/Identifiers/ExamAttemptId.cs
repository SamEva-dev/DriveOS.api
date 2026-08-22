namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for one concrete examination attempt.</summary>
public readonly record struct ExamAttemptId(Guid Value)
{
    public static ExamAttemptId New() => new(Guid.NewGuid());
    public static ExamAttemptId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
