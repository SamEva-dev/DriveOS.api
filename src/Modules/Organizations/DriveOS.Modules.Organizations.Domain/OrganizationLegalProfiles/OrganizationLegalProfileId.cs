namespace DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;

public readonly record struct OrganizationLegalProfileId(Guid Value)
{
    public static OrganizationLegalProfileId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
