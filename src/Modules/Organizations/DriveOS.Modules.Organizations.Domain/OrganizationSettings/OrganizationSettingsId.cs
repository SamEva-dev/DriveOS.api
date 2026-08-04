namespace DriveOS.Modules.Organizations.Domain.OrganizationSettings;

public readonly record struct OrganizationSettingsId(Guid Value)
{
    public static OrganizationSettingsId New() => new(Guid.NewGuid());
    public static OrganizationSettingsId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
