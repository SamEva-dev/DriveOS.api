namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Cross-context reference to a document owned by BC-06 Contracts &amp; Documents.</summary>
public readonly record struct DocumentId(Guid Value)
{
    public static DocumentId New() => new(Guid.NewGuid());
    public static DocumentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
