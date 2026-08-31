namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for a professional marketplace profile in BC-13 Professional Marketplace.</summary>
public readonly record struct ProfessionalProfileId(Guid Value)
{
    public static ProfessionalProfileId New() => new(Guid.NewGuid());
    public static ProfessionalProfileId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
