namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct ServiceStatementId(Guid Value)
{
    public static ServiceStatementId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
