namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionReportRevisionId(Guid Value)
{
    public static TrainingSessionReportRevisionId New() => new(Guid.NewGuid());
    public static TrainingSessionReportRevisionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
}
