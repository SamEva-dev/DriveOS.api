namespace DriveOS.Modules.Organizations.Domain.RegulatoryIntegrations;

public readonly record struct RegulatoryIntegrationConnectionId(Guid Value)
{
    public static RegulatoryIntegrationConnectionId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
