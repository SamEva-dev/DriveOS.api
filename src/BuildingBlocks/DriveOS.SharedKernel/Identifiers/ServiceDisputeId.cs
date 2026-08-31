namespace DriveOS.SharedKernel.Identifiers;

public readonly record struct ServiceDisputeId(Guid Value)
{
    public static ServiceDisputeId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
