namespace DriveOS.Modules.Organizations.Domain.OrganizationSequences;

public readonly record struct OrganizationSequenceId(Guid Value)
{
    public bool IsEmpty => Value == Guid.Empty;

    public static OrganizationSequenceId New() => new(Guid.NewGuid());
}
