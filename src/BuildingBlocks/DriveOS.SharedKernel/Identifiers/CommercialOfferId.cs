namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CommercialOfferId(Guid Value)
{
    public static CommercialOfferId New() => new(Guid.NewGuid());

    public static CommercialOfferId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
