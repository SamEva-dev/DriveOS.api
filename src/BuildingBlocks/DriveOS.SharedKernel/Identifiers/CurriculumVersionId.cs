namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CurriculumVersionId(Guid Value)
{
    public static CurriculumVersionId New() => new(Guid.NewGuid());

    public static CurriculumVersionId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
