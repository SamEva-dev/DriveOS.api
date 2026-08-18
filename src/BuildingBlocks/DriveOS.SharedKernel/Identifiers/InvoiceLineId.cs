namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct InvoiceLineId(Guid Value)
{
    public static InvoiceLineId New() => new(Guid.NewGuid());
    public static InvoiceLineId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
