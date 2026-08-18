namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct FundingAllocationId(Guid Value)
{
    public static FundingAllocationId New() => new(Guid.NewGuid());
    public static FundingAllocationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
