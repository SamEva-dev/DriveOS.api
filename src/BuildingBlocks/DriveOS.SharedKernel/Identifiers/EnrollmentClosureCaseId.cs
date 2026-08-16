namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct EnrollmentClosureCaseId(Guid Value)
{
    public static EnrollmentClosureCaseId New() => new(Guid.NewGuid());
    public static EnrollmentClosureCaseId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(EnrollmentClosureCaseId id) => id.Value;
    public static explicit operator EnrollmentClosureCaseId(Guid value) => new(value);
}
