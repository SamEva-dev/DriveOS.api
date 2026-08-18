namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingCreditMovementId(Guid Value)
{
    public static TrainingCreditMovementId New() => new(Guid.NewGuid());
    public static TrainingCreditMovementId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
