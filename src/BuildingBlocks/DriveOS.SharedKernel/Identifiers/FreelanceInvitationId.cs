namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct FreelanceInvitationId(Guid Value)
{
    public static FreelanceInvitationId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
