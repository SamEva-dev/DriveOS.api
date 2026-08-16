namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct StudentDocumentId(Guid Value)
{
    public static StudentDocumentId New() => new(Guid.NewGuid());
    public static StudentDocumentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(StudentDocumentId id) => id.Value;
    public static explicit operator StudentDocumentId(Guid value) => new(value);
}
