namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct OrganizationId(Guid Value)
{
    public static OrganizationId New() =>
        new(Guid.NewGuid());

    public static OrganizationId Empty =>
        new(Guid.Empty);

    public bool IsEmpty =>
        Value == Guid.Empty;

    public override string ToString() =>
        Value.ToString();
}