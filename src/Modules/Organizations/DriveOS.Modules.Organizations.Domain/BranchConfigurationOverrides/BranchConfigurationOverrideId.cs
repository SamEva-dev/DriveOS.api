namespace DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;

public readonly record struct BranchConfigurationOverrideId(Guid Value)
{
    public static BranchConfigurationOverrideId New() => new(Guid.NewGuid());

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
