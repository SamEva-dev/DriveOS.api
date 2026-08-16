namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct StudentRelationshipId(Guid Value)
{
    public static StudentRelationshipId New() => new(Guid.NewGuid());
    public static StudentRelationshipId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(StudentRelationshipId id) => id.Value;
    public static explicit operator StudentRelationshipId(Guid value) => new(value);
}
