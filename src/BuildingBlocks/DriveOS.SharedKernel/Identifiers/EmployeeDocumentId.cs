namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct EmployeeDocumentId(Guid Value)
{
    public static EmployeeDocumentId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
