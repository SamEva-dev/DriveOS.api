namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingContractId(Guid Value)
{
    public static TrainingContractId New() => new(Guid.NewGuid());

    public static TrainingContractId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
