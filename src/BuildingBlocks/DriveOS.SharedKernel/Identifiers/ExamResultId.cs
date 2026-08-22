namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier used by BC-11 Exams &amp; Certification.</summary>
public readonly record struct ExamResultId(Guid Value)
{
    public static ExamResultId New() => new(Guid.NewGuid());
    public static ExamResultId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
