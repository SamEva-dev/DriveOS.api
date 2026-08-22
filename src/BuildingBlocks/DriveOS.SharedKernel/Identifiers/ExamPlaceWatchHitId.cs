namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for a place availability detected by a watcher subscription.</summary>
public readonly record struct ExamPlaceWatchHitId(Guid Value)
{
    public static ExamPlaceWatchHitId New() => new(Guid.NewGuid());
    public static ExamPlaceWatchHitId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
