namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ServiceEntryId(Guid Value)
{
    public static ServiceEntryId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
