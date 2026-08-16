namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct EnrollmentChecklistId(Guid Value)
{
    public static EnrollmentChecklistId New() => new(Guid.NewGuid());
    public static EnrollmentChecklistId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(EnrollmentChecklistId id) => id.Value;
    public static explicit operator EnrollmentChecklistId(Guid value) => new(value);
}
