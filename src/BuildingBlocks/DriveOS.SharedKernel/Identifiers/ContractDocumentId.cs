namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ContractDocumentId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static ContractDocumentId New() => new(Guid.NewGuid());
}
