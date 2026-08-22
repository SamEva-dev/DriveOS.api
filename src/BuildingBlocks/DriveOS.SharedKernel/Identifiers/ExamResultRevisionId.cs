namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for one immutable revision of an examination result.</summary>
public readonly record struct ExamResultRevisionId(Guid Value)
{
    public static ExamResultRevisionId New() => new(Guid.NewGuid());
    public static ExamResultRevisionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
