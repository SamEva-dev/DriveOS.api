namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ExternalTransferCaseId(Guid Value)
{
    public static ExternalTransferCaseId New() => new(Guid.NewGuid());
    public static ExternalTransferCaseId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(ExternalTransferCaseId id) => id.Value;
    public static explicit operator ExternalTransferCaseId(Guid value) => new(value);
}
