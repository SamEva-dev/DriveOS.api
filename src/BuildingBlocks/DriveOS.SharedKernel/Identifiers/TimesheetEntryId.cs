namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct TimesheetEntryId(Guid Value)
{
    public static TimesheetEntryId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
