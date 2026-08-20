namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct WaitingListEntryId(Guid Value)
{
    public static WaitingListEntryId New() => new(Guid.NewGuid());
    public static WaitingListEntryId Empty => new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
}
