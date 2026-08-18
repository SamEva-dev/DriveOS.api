namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct FundingPlanId(Guid Value)
{
    public static FundingPlanId New() => new(Guid.NewGuid());
    public static FundingPlanId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
