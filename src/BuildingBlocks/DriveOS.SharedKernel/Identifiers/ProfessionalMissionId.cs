namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ProfessionalMissionId(Guid Value)
{
    public static ProfessionalMissionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
