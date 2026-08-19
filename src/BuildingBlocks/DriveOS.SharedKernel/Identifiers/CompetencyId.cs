namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CompetencyId(Guid Value)
{
    public static CompetencyId New() => new(Guid.NewGuid());

    public static CompetencyId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
