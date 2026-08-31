namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ProfessionalDocumentId(Guid Value)
{
    public static ProfessionalDocumentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
