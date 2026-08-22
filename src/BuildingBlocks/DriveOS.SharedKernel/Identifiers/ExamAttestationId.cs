namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamAttestationId(Guid Value)
{
    public static ExamAttestationId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
