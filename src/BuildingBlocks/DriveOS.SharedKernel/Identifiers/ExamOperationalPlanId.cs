namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an exam operational planning aggregate.</summary>
public readonly record struct ExamOperationalPlanId(Guid Value)
{
    public static ExamOperationalPlanId New() => new(Guid.NewGuid());
    public static ExamOperationalPlanId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
