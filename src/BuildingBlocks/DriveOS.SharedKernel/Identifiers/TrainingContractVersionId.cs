namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingContractVersionId(Guid Value)
{
    public static TrainingContractVersionId New() => new(Guid.NewGuid());

    public static TrainingContractVersionId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
