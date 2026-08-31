namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ProfessionalCredentialId(Guid Value)
{
    public static ProfessionalCredentialId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
