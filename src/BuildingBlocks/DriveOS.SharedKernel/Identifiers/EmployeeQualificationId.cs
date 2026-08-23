namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct EmployeeQualificationId(Guid Value)
{
    public static EmployeeQualificationId New() => new(Guid.NewGuid());
    public static EmployeeQualificationId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
