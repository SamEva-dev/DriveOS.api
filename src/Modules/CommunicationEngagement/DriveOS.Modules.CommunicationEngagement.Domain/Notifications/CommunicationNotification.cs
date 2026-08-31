using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Domain.Notifications;

/// <summary>
/// Durable in-app notification owned by BC-15.
/// Business BCs provide a stable template key, recipient principal and contextual payload;
/// channel delivery (email/push/etc.) remains a Communication concern.
/// </summary>
public sealed class CommunicationNotification:AggregateRoot<CommunicationNotificationId>,IAuditableEntity
{
    private CommunicationNotification(){}
    private CommunicationNotification(
        CommunicationNotificationId id,
        CommunicationNotificationRecipientType recipientType,
        Guid recipientId,
        OrganizationId? organizationId,
        string category,
        string templateKey,
        string deduplicationKey,
        string payloadJson,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? emailAddress,
        string? cultureCode,
        bool inAppVisible):base(id)
    {
        RecipientType=recipientType;
        RecipientId=recipientId;
        OrganizationId=organizationId;
        Category=Token(category,80);
        TemplateKey=Template(templateKey);
        DeduplicationKey=Token(deduplicationKey,180);
        PayloadJson=payloadJson;
        RelatedEntityType=OptionalToken(relatedEntityType,80);
        RelatedEntityId=relatedEntityId;
        EmailAddress=NormalizeOptional(emailAddress,320);
        CultureCode=NormalizeOptional(cultureCode,16);
        InAppVisible=inAppVisible;
        Status=CommunicationNotificationStatus.Unread;
        EmailStatus=CommunicationNotificationEmailStatus.NotRequested;
    }

    public CommunicationNotificationRecipientType RecipientType{get;private set;}
    public Guid RecipientId{get;private set;}
    public OrganizationId? OrganizationId{get;private set;}
    public string Category{get;private set;}=string.Empty;
    public string TemplateKey{get;private set;}=string.Empty;
    public string DeduplicationKey{get;private set;}=string.Empty;
    public string PayloadJson{get;private set;}="{}";
    public string? RelatedEntityType{get;private set;}
    public Guid? RelatedEntityId{get;private set;}
    public CommunicationNotificationStatus Status{get;private set;}
    public bool InAppVisible{get;private set;}
    public string? EmailAddress{get;private set;}
    public string? CultureCode{get;private set;}
    public CommunicationNotificationEmailStatus EmailStatus{get;private set;}
    public Guid? EmailMessageId{get;private set;}
    public DateTimeOffset? EmailQueuedAtUtc{get;private set;}
    public DateTimeOffset? ReadAtUtc{get;private set;}
    public DateTimeOffset? DismissedAtUtc{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<CommunicationNotification> Create(
        CommunicationNotificationId id,
        CommunicationNotificationRecipientType recipientType,
        Guid recipientId,
        OrganizationId? organizationId,
        string category,
        string templateKey,
        string deduplicationKey,
        string payloadJson,
        string? relatedEntityType,
        Guid? relatedEntityId,
        string? emailAddress,
        string? cultureCode,
        bool inAppVisible,
        DateTimeOffset now,
        UserId? actor=null)
    {
        if(id.IsEmpty||recipientId==Guid.Empty)
            return Result.Failure<CommunicationNotification>(CommunicationNotificationErrors.InvalidIdentifier);
        if(string.IsNullOrWhiteSpace(category)||string.IsNullOrWhiteSpace(templateKey)||
           string.IsNullOrWhiteSpace(deduplicationKey)||string.IsNullOrWhiteSpace(payloadJson))
            return Result.Failure<CommunicationNotification>(CommunicationNotificationErrors.InvalidContent);

        var x=new CommunicationNotification(id,recipientType,recipientId,organizationId,category,templateKey,
            deduplicationKey,payloadJson,relatedEntityType,relatedEntityId,emailAddress,cultureCode,inAppVisible);
        x.SetCreatedAudit(now,actor);
        return Result.Success(x);
    }


    public Result MarkEmailQueued(Guid emailMessageId,DateTimeOffset now)
    {
        if(emailMessageId==Guid.Empty)
            return Result.Failure(CommunicationNotificationErrors.InvalidContent);
        EmailStatus=CommunicationNotificationEmailStatus.Queued;
        EmailMessageId=emailMessageId;
        EmailQueuedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public Result MarkEmailSkipped(DateTimeOffset now)
    {
        EmailStatus=CommunicationNotificationEmailStatus.Skipped;
        SetModifiedAudit(now,null);
        return Result.Success();
    }

    public Result MarkRead(DateTimeOffset now,UserId actor)
    {
        if(Status==CommunicationNotificationStatus.Dismissed)
            return Result.Failure(CommunicationNotificationErrors.InvalidTransition);
        Status=CommunicationNotificationStatus.Read;
        ReadAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public Result Dismiss(DateTimeOffset now,UserId actor)
    {
        if(Status==CommunicationNotificationStatus.Dismissed)
            return Result.Failure(CommunicationNotificationErrors.InvalidTransition);
        Status=CommunicationNotificationStatus.Dismissed;
        DismissedAtUtc=now.ToUniversalTime();
        SetModifiedAudit(now,actor);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}

    private static string Token(string? value,int max)
    {
        string s=(value??string.Empty).Trim().ToUpperInvariant();
        return s.Length<=max?s:s[..max];
    }
    private static string Template(string? value)
    {
        string s=(value??string.Empty).Trim();
        return s.Length<=180?s:s[..180];
    }
    private static string? OptionalToken(string? value,int max)
    {
        if(string.IsNullOrWhiteSpace(value))return null;
        string s=value.Trim().ToUpperInvariant();
        return s.Length<=max?s:s[..max];
    }

    private static string? NormalizeOptional(string? value,int max)
    {
        if(string.IsNullOrWhiteSpace(value))return null;
        string s=value.Trim();
        return s.Length<=max?s:s[..max];
    }
}

public enum CommunicationNotificationRecipientType{User=1,Organization=2}
public enum CommunicationNotificationStatus{Unread=1,Read=2,Dismissed=3}
public enum CommunicationNotificationEmailStatus{NotRequested=1,Queued=2,Skipped=3}
