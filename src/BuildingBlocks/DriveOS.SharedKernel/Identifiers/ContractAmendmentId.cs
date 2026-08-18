namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ContractAmendmentId(Guid Value)
{
    public static ContractAmendmentId New() => new(Guid.NewGuid());
    public static ContractAmendmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
