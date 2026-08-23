namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an employee job-position assignment.</summary>
public readonly record struct EmployeeJobPositionAssignmentId(Guid Value)
{
    public static EmployeeJobPositionAssignmentId New() => new(Guid.NewGuid());
    public static EmployeeJobPositionAssignmentId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
