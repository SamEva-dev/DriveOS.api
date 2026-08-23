namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct InstructorAuthorizationId(Guid Value)
{
    public static InstructorAuthorizationId New() => new(Guid.NewGuid());
    public static InstructorAuthorizationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
