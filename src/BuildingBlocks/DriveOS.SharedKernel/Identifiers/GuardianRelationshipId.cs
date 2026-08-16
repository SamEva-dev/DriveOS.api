namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct GuardianRelationshipId(Guid Value)
{
    public static GuardianRelationshipId New() => new(Guid.NewGuid());
    public static GuardianRelationshipId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(GuardianRelationshipId id) => id.Value;
    public static explicit operator GuardianRelationshipId(Guid value) => new(value);
}
