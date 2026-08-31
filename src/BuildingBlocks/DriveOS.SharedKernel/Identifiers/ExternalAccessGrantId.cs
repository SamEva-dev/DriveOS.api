namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ExternalAccessGrantId(Guid Value)
{
    public static ExternalAccessGrantId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
