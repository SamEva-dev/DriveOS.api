namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct CrmTaskId(Guid Value)
{
    public static CrmTaskId New() => new(Guid.NewGuid());
    public static CrmTaskId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
