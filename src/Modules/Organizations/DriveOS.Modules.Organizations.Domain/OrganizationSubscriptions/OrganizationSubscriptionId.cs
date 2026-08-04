namespace DriveOS.Modules.Organizations.Domain.Subscriptions;

public readonly record struct OrganizationSubscriptionId(Guid Value)
{
    public static OrganizationSubscriptionId New() => new(Guid.NewGuid());
    public static OrganizationSubscriptionId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
