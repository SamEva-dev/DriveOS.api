namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct EnrollmentSuspensionId(Guid Value)
{
    public static EnrollmentSuspensionId New() => new(Guid.NewGuid());
    public static EnrollmentSuspensionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(EnrollmentSuspensionId id) => id.Value;
    public static explicit operator EnrollmentSuspensionId(Guid value) => new(value);
}
