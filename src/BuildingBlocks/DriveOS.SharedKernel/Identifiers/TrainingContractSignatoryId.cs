namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct TrainingContractSignatoryId(Guid Value)
{
    public static TrainingContractSignatoryId New() => new(Guid.NewGuid());
    public static TrainingContractSignatoryId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
