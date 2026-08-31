namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct CommunicationNotificationId(Guid Value)
{
    public static CommunicationNotificationId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
