namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct LeaveRequestId(Guid Value)
{
    public static LeaveRequestId New() => new(Guid.NewGuid());
    public static LeaveRequestId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
