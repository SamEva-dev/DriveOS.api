namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ProfessionalComplianceRequirementId(Guid Value)
{
    public static ProfessionalComplianceRequirementId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
