namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct LeadId(Guid Value)
{
    public static LeadId New() => new(Guid.NewGuid());
    public static LeadId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
