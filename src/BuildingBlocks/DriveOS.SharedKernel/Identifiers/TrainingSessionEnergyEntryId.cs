namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionEnergyEntryId(Guid Value)
{
    public static TrainingSessionEnergyEntryId New() => new(Guid.NewGuid());
    public static TrainingSessionEnergyEntryId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
}
