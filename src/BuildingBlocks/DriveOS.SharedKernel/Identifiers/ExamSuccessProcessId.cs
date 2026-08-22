namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamSuccessProcessId(Guid Value)
{
    public static ExamSuccessProcessId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
