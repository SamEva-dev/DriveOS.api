namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CurriculumModuleId(Guid Value)
{
    public static CurriculumModuleId New() => new(Guid.NewGuid());

    public static CurriculumModuleId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
