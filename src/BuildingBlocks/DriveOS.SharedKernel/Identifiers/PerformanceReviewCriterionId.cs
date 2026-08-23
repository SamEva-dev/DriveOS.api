namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct PerformanceReviewCriterionId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static PerformanceReviewCriterionId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
