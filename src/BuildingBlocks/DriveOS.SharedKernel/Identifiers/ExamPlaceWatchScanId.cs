namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an examination-place watcher scan.</summary>
public readonly record struct ExamPlaceWatchScanId(Guid Value)
{
    public static ExamPlaceWatchScanId New() => new(Guid.NewGuid());
    public static ExamPlaceWatchScanId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
