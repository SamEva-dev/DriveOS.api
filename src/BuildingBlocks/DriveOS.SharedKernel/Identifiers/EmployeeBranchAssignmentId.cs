namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct EmployeeBranchAssignmentId(Guid Value)
{
    public static EmployeeBranchAssignmentId New() => new(Guid.NewGuid());
    public static EmployeeBranchAssignmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
