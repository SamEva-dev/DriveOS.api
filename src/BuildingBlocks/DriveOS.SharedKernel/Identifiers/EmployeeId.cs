namespace DriveOS.SharedKernel.Identifiers;

/// <summary>Strongly typed identifier for an employment relationship in BC-12 Workforce &amp; HR.</summary>
public readonly record struct EmployeeId(Guid Value)
{
    public static EmployeeId New() => new(Guid.NewGuid());
    public static EmployeeId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
