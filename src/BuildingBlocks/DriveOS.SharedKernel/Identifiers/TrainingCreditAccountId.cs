namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingCreditAccountId(Guid Value)
{
    public static TrainingCreditAccountId New() => new(Guid.NewGuid());
    public static TrainingCreditAccountId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
