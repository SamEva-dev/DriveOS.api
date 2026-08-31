namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ProfessionalOpportunityId(Guid Value)
{
    public static ProfessionalOpportunityId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
