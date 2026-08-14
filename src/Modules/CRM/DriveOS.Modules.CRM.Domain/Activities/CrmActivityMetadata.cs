namespace DriveOS.Modules.CRM.Domain.Activities;

public enum CrmActivityOrigin { Manual = 0, Imported = 1, System = 2 }
public enum CrmActivitySyncStatus { NotApplicable = 0, Pending = 1, Synchronized = 2, Failed = 3, Abandoned = 4 }

public sealed record CrmActivityMetadata(string? Result, int? DurationMinutes,
    bool IsInternal, bool IsUnfollowed, bool RequiresRegularization,
    CrmActivityOrigin Origin, CrmActivitySyncStatus SyncStatus,
    string? ExternalId, string? IdempotencyKey, string? SyncErrorKey,
    int SyncAttemptCount, DateTimeOffset? LastSyncAttemptAtUtc,
    string? AttachmentName, string? AttachmentReference)
{
    public static CrmActivityMetadata Manual(string? result = null, int? durationMinutes = null,
        bool isInternal = false, bool isUnfollowed = false, bool requiresRegularization = false,
        string? attachmentName = null, string? attachmentReference = null) =>
        new(result, durationMinutes, isInternal, isUnfollowed, requiresRegularization,
            CrmActivityOrigin.Manual, CrmActivitySyncStatus.NotApplicable, null, null,
            null, 0, null, attachmentName, attachmentReference);

    public static CrmActivityMetadata Imported(string externalId, string idempotencyKey,
        CrmActivitySyncStatus syncStatus, DateTimeOffset lastSyncAttemptAtUtc,
        string? syncErrorKey = null, string? result = null,
        int? durationMinutes = null, bool requiresRegularization = false,
        string? attachmentName = null, string? attachmentReference = null) =>
        new(result, durationMinutes, false, false, requiresRegularization,
            CrmActivityOrigin.Imported, syncStatus, (externalId ?? string.Empty).Trim(),
            (idempotencyKey ?? string.Empty).Trim(),
            string.IsNullOrWhiteSpace(syncErrorKey) ? null : syncErrorKey.Trim(), 1,
            lastSyncAttemptAtUtc.ToUniversalTime(), attachmentName, attachmentReference);
}
