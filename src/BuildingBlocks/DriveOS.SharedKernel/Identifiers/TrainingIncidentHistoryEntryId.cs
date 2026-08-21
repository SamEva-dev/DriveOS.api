namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingIncidentHistoryEntryId(Guid Value)
{
    public static TrainingIncidentHistoryEntryId New() => new(Guid.NewGuid());
    public static TrainingIncidentHistoryEntryId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
