namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ContractDocumentVersionId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static ContractDocumentVersionId New() => new(Guid.NewGuid());
}
