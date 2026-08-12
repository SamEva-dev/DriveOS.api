namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct LeadConversionId(Guid Value)
{
    public static LeadConversionId New() => new(Guid.NewGuid());
    public static LeadConversionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
