namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExamProviderConnectionId(Guid Value)
{
    public static ExamProviderConnectionId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
