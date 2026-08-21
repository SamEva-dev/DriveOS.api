namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionOdometerReadingId(Guid Value)
{
    public static TrainingSessionOdometerReadingId New() => new(Guid.NewGuid());
    public static TrainingSessionOdometerReadingId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
