namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an immutable official convocation revision.</summary>
public readonly record struct ExamConvocationRevisionId(Guid Value)
{
    public static ExamConvocationRevisionId New() => new(Guid.NewGuid());
    public static ExamConvocationRevisionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
