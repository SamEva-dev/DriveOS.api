namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct TimesheetId(Guid Value)
{
    public static TimesheetId New() => new(Guid.NewGuid());
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
