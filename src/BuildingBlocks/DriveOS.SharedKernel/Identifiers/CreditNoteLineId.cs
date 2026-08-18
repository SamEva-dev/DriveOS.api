namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CreditNoteLineId(Guid Value)
{
    public static CreditNoteLineId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
