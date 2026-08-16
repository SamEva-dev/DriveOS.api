namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct AdministrativeCaseId(Guid Value)
{
    public static AdministrativeCaseId New() => new(Guid.NewGuid());
    public static AdministrativeCaseId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(AdministrativeCaseId id) => id.Value;
    public static explicit operator AdministrativeCaseId(Guid value) => new(value);
}
