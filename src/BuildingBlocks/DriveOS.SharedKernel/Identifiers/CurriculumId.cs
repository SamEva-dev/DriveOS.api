namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CurriculumId(Guid Value)
{
    public static CurriculumId New() => new(Guid.NewGuid());

    public static CurriculumId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
