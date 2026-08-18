namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CreditNoteId(Guid Value)
{
    public static CreditNoteId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
