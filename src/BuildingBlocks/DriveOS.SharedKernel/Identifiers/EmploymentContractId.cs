namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct EmploymentContractId(Guid Value)
{
    public static EmploymentContractId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
