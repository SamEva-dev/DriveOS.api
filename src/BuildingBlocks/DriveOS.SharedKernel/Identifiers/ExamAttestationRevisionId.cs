namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamAttestationRevisionId(Guid Value)
{
    public static ExamAttestationRevisionId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
