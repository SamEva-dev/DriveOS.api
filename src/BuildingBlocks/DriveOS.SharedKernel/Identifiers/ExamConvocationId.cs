namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an exam convocation aggregate.</summary>
public readonly record struct ExamConvocationId(Guid Value)
{
    public static ExamConvocationId New() => new(Guid.NewGuid());
    public static ExamConvocationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
