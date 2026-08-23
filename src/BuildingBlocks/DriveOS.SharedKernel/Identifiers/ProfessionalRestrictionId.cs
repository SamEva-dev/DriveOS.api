namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ProfessionalRestrictionId(Guid Value)
{
    public static ProfessionalRestrictionId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
