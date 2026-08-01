namespace DriveOS.Modules.Organizations.Domain
    .BranchAssignments;

public readonly record struct BranchUserAssignmentId(
    Guid Value)
{
    public static BranchUserAssignmentId New() =>
        new(Guid.NewGuid());

    public static BranchUserAssignmentId Empty =>
        new(Guid.Empty);

    public bool IsEmpty =>
        Value == Guid.Empty;

    public override string ToString() =>
        Value.ToString();
}