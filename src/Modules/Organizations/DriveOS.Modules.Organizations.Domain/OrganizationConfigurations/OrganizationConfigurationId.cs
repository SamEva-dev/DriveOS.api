namespace DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;

public readonly record struct OrganizationConfigurationId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;

    public static OrganizationConfigurationId New() => new(Guid.NewGuid());
}
