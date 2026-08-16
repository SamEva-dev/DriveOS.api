namespace DriveOS.Modules.Organizations.Domain.OrganizationClosures;

public readonly record struct OrganizationClosureId(Guid Value)
{
    public static OrganizationClosureId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
