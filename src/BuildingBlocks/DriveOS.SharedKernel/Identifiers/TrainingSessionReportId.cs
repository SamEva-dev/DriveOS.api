namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionReportId(Guid Value)
{
    public static TrainingSessionReportId New() => new(Guid.NewGuid());
    public static TrainingSessionReportId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
