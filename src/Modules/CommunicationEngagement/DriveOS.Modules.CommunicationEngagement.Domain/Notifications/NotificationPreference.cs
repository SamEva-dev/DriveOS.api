using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Domain.Notifications;

/// <summary>
/// Per-user notification preference for a stable business category.
/// In-app notifications stay available by default; email can be disabled independently.
/// </summary>
public sealed class NotificationPreference:AggregateRoot<NotificationPreferenceId>,IAuditableEntity
{
    private NotificationPreference(){}

    private NotificationPreference(
        NotificationPreferenceId id,
        UserId userId,
        string category,
        bool inAppEnabled,
        bool emailEnabled):base(id)
    {
        UserId=userId;
        Category=NormalizeCategory(category);
        InAppEnabled=inAppEnabled;
        EmailEnabled=emailEnabled;
    }

    public UserId UserId{get;private set;}
    public string Category{get;private set;}=string.Empty;
    public bool InAppEnabled{get;private set;}
    public bool EmailEnabled{get;private set;}
    public DateTimeOffset CreatedAtUtc{get;private set;}
    public UserId? CreatedByUserId{get;private set;}
    public DateTimeOffset? LastModifiedAtUtc{get;private set;}
    public UserId? LastModifiedByUserId{get;private set;}

    public static Result<NotificationPreference> Create(
        NotificationPreferenceId id,
        UserId userId,
        string category,
        bool inAppEnabled,
        bool emailEnabled,
        DateTimeOffset now)
    {
        if(id.IsEmpty||userId.IsEmpty)
            return Result.Failure<NotificationPreference>(NotificationPreferenceErrors.InvalidIdentifier);

        string normalized=NormalizeCategory(category);
        if(normalized.Length is <2 or >80)
            return Result.Failure<NotificationPreference>(NotificationPreferenceErrors.InvalidCategory);

        var pref=new NotificationPreference(id,userId,normalized,inAppEnabled,emailEnabled);
        pref.SetCreatedAudit(now,userId);
        return Result.Success(pref);
    }

    public Result Update(bool inAppEnabled,bool emailEnabled,DateTimeOffset now)
    {
        InAppEnabled=inAppEnabled;
        EmailEnabled=emailEnabled;
        SetModifiedAudit(now,UserId);
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at,UserId? actor){CreatedAtUtc=at.ToUniversalTime();CreatedByUserId=actor;}
    public void SetModifiedAudit(DateTimeOffset at,UserId? actor){LastModifiedAtUtc=at.ToUniversalTime();LastModifiedByUserId=actor;}
    private static string NormalizeCategory(string? value)=>(value??string.Empty).Trim().ToUpperInvariant();
}

public static class NotificationPreferenceErrors
{
    public static readonly Error NotFound=Error.NotFound("Communication.NotificationPreferences.NotFound","errors.communication.notificationPreferences.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("Communication.NotificationPreferences.InvalidIdentifier","errors.communication.notificationPreferences.invalidIdentifier");
    public static readonly Error InvalidCategory=Error.Validation("Communication.NotificationPreferences.InvalidCategory","errors.communication.notificationPreferences.invalidCategory");
}
