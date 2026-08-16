namespace DriveOS.Modules.Organizations.Domain.OrganizationRepresentatives;

public readonly record struct OrganizationRepresentativeId(Guid Value)
{
    public static OrganizationRepresentativeId New() => new(Guid.NewGuid());

    public static OrganizationRepresentativeId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
