namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamRemediationRequestId(Guid Value)
{
    public static ExamRemediationRequestId New() => new(Guid.NewGuid());
    public static ExamRemediationRequestId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
