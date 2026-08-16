namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AssessmentSessionId(Guid Value)
{
    public static AssessmentSessionId New() => new(Guid.NewGuid());

    public static AssessmentSessionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
