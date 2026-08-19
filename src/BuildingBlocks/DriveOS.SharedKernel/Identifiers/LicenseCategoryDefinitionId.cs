namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct LicenseCategoryDefinitionId(Guid Value)
{
    public static LicenseCategoryDefinitionId New() => new(Guid.NewGuid());

    public static LicenseCategoryDefinitionId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
