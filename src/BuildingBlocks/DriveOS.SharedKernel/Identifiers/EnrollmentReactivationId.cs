namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct EnrollmentReactivationId(Guid Value)
{
    public static EnrollmentReactivationId New() => new(Guid.NewGuid());
    public static EnrollmentReactivationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(EnrollmentReactivationId id) => id.Value;
    public static explicit operator EnrollmentReactivationId(Guid value) => new(value);
}
