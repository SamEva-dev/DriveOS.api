namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct OfferInteractionId(Guid Value)
{
    public static OfferInteractionId New() => new(Guid.NewGuid());

    public static OfferInteractionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
