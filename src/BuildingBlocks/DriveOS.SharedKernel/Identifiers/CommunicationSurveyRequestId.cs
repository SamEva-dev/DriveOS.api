namespace DriveOS.SharedKernel.Identifiers;
public readonly record struct CommunicationSurveyRequestId(Guid Value)
{
    public static CommunicationSurveyRequestId Empty=>new(Guid.Empty);
    public bool IsEmpty=>Value==Guid.Empty;
    public override string ToString()=>Value.ToString();
}
