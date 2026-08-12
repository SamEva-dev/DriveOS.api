namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AssessmentSessionRevisionId(Guid Value)
{
    public static AssessmentSessionRevisionId New() => new(Guid.NewGuid());
    public static AssessmentSessionRevisionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
