namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingSessionObservationId(Guid Value)
{
    public static TrainingSessionObservationId New() => new(Guid.NewGuid());
    public static TrainingSessionObservationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
