namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct PerformanceReviewId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;
    public static PerformanceReviewId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();
}
