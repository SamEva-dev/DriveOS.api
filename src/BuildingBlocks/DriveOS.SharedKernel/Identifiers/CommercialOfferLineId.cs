namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CommercialOfferLineId(Guid Value)
{
    public static CommercialOfferLineId New() => new(Guid.NewGuid());

    public static CommercialOfferLineId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
