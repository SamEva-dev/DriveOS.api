namespace DriveOS.Modules.Organizations.Domain.Branches;

public readonly record struct BranchManagerAssignmentId(Guid Value)
{
    public static BranchManagerAssignmentId New() => new(Guid.NewGuid());

    public static BranchManagerAssignmentId Empty => new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();
}
