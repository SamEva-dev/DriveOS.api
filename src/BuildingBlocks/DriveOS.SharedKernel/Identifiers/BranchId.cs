namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct BranchId(Guid Value)
{
    public static BranchId New() => new(Guid.NewGuid());

    public static BranchId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
