using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Domain.Surveys;

/// <summary>
/// Durable request to launch a Communication-owned survey.
/// Business BCs provide the survey type, recipient and context; questionnaire design and responses remain BC-15 concerns.
/// </summary>
public sealed class CommunicationSurveyRequest
    :AggregateRoot<CommunicationSurveyRequestId>,IAuditableEntity
{
    private CommunicationSurveyRequest(){}

    private CommunicationSurveyRequest(
        CommunicationSurveyRequestId id,
        UserId recipientUserId,
        OrganizationId organizationId,
        string surveyType,
        string deduplicationKey,
        string relatedEntityType,
        Guid relatedEntityId,
        string payloadJson):base(id)
    {
        RecipientUserId=recipientUserId;
        OrganizationId=organizationId;
        SurveyType=Token(surveyType,80);
        DeduplicationKey=deduplicationKey.Trim();
        RelatedEntityType=Token(relatedEntityType,80);
        RelatedEntityId=relatedEntityId;
        PayloadJson=payloadJson;
        Status=CommunicationSurveyRequestStatus.Pending;
    }

    public UserId RecipientUserId{get;private set;}
    public OrganizationId OrganizationId{get;private set;}
    public string SurveyType{get;private set;}=string.Empty;
    public string DeduplicationKey{get;private set;}=string.Empty;
    public string RelatedEntityType{get;private set;}=string.Empty;
    public Guid RelatedEntityId{get;private set;}
    public string PayloadJson{get;private set;}="{}";
    public CommunicationSurveyRequestStatus Status{get;private set;}

    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<CommunicationSurveyRequest> Create(
        CommunicationSurveyRequestId id,
        UserId recipientUserId,
        OrganizationId organizationId,
        string surveyType,
        string deduplicationKey,
        string relatedEntityType,
        Guid relatedEntityId,
        string payloadJson,
        DateTimeOffset now)
    {
        if(id.IsEmpty||recipientUserId.IsEmpty||organizationId.IsEmpty||relatedEntityId==Guid.Empty)
            return Result.Failure<CommunicationSurveyRequest>(
                Error.Validation("Communication.Surveys.InvalidIdentifier","errors.communication.surveys.invalidIdentifier"));

        if(string.IsNullOrWhiteSpace(surveyType)||string.IsNullOrWhiteSpace(deduplicationKey)||
           string.IsNullOrWhiteSpace(relatedEntityType)||string.IsNullOrWhiteSpace(payloadJson))
            return Result.Failure<CommunicationSurveyRequest>(
                Error.Validation("Communication.Surveys.InvalidContent","errors.communication.surveys.invalidContent"));

        var x=new CommunicationSurveyRequest(
            id,recipientUserId,organizationId,surveyType,deduplicationKey,
            relatedEntityType,relatedEntityId,payloadJson);
        x.SetCreatedAudit(now,null);
        return Result.Success(x);
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}

    private static string Token(string? value,int max)
    {
        string s=(value??string.Empty).Trim().ToUpperInvariant();
        return s.Length<=max?s:s[..max];
    }
}

public enum CommunicationSurveyRequestStatus
{
    Pending=1,
    Launched=2,
    Completed=3,
    Cancelled=4
}
