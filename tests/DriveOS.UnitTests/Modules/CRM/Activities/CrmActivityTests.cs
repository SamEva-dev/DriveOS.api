using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.UnitTests.Modules.CRM.Activities;

public sealed class CrmActivityTests
{
    private static readonly OrganizationId OrganizationId = new(Guid.NewGuid());
    private static readonly LeadId LeadId = new(Guid.NewGuid());

    [Fact]
    public void Create_ShouldNormalizeContent_AndStoreUtcDate()
    {
        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Call, CrmActivityDirection.Outbound, "  Appel de suivi  ",
            "  Le prospect est intéressé.  ", DateTimeOffset.Now);

        Assert.True(result.IsSuccess);
        Assert.Equal("Appel de suivi", result.Value.Subject);
        Assert.Equal("Le prospect est intéressé.", result.Value.Details);
        Assert.Equal(TimeSpan.Zero, result.Value.OccurredAtUtc.Offset);
    }

    [Fact]
    public void Create_NoteWithDirection_ShouldFail()
    {
        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Note, CrmActivityDirection.Outbound, "Note", null,
            DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(CrmActivityErrors.DirectionNotAllowed, result.Error);
    }

    [Fact]
    public void Create_AllowsAnUnattachedBranchVisit()
    {
        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, null,
            CrmActivityType.BranchVisit, CrmActivityDirection.Inbound,
            "Visite spontanée", null, DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.LeadId);
    }

    [Fact]
    public void Invalidate_KeepsTheActivityAndStoresAuditInformation()
    {
        CrmActivity activity = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Email, CrmActivityDirection.Outbound, "Relance", null,
            DateTimeOffset.UtcNow).Value;
        UserId userId = UserId.New();
        var result = activity.Invalidate("Activité créée par erreur", userId, DateTimeOffset.UtcNow);
        Assert.True(result.IsSuccess);
        Assert.Equal(userId, activity.InvalidatedByUserId);
        Assert.NotNull(activity.InvalidatedAtUtc);
    }

    [Fact]
    public void RetrySynchronization_IsOnlyAllowedForFailedImports()
    {
        var metadata = new CrmActivityMetadata(null, null, false, false, true,
            CrmActivityOrigin.Imported, CrmActivitySyncStatus.Failed, "ext-1", "key-1",
            "Crm.Sync.LeadNotFound", 1, DateTimeOffset.UtcNow, null, null);
        CrmActivity activity = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Call, CrmActivityDirection.Outbound, "Import", null,
            DateTimeOffset.UtcNow, null, metadata).Value;
        Assert.True(activity.RetrySynchronization(DateTimeOffset.UtcNow).IsSuccess);
        Assert.Equal(CrmActivitySyncStatus.Pending, activity.Metadata.SyncStatus);
        Assert.Equal(2, activity.Metadata.SyncAttemptCount);
    }

    [Fact]
    public void AttachToLead_ShouldAttachAnUnattachedActivityOnlyOnce()
    {
        CrmActivity activity = CrmActivity.Create(CrmActivityId.New(), OrganizationId, null,
            CrmActivityType.BranchVisit, CrmActivityDirection.Inbound, "Visite", null,
            DateTimeOffset.UtcNow).Value;

        Assert.True(activity.AttachToLead(LeadId).IsSuccess);
        Assert.Equal(LeadId, activity.LeadId);
        Assert.True(activity.AttachToLead(new LeadId(Guid.NewGuid())).IsFailure);
    }

    [Fact]
    public void Invalidate_ShouldRejectASecondInvalidation()
    {
        CrmActivity activity = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Call, CrmActivityDirection.Outbound, "Appel", null,
            DateTimeOffset.UtcNow).Value;
        UserId userId = UserId.New();

        Assert.True(activity.Invalidate("Erreur de saisie", userId, DateTimeOffset.UtcNow).IsSuccess);
        var second = activity.Invalidate("Nouvelle tentative", userId, DateTimeOffset.UtcNow);

        Assert.True(second.IsFailure);
        Assert.Equal(CrmActivityErrors.AlreadyInvalidated, second.Error);
    }

    [Fact]
    public void Create_ShouldRejectAnInvalidDuration()
    {
        CrmActivityMetadata metadata = CrmActivityMetadata.Manual(durationMinutes: 1441);

        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, LeadId,
            CrmActivityType.Call, CrmActivityDirection.Outbound, "Appel", null,
            DateTimeOffset.UtcNow, null, metadata);

        Assert.True(result.IsFailure);
        Assert.Equal(CrmActivityErrors.DurationInvalid, result.Error);
    }

    [Fact]
    public void Create_ShouldStoreImportedSynchronizationMetadata()
    {
        DateTimeOffset attemptAt = DateTimeOffset.UtcNow;
        CrmActivityMetadata metadata = CrmActivityMetadata.Imported("external-42", "source:42",
            CrmActivitySyncStatus.Failed, attemptAt, "Crm.Sync.UnknownLead",
            requiresRegularization: true);

        var result = CrmActivity.Create(CrmActivityId.New(), OrganizationId, null,
            CrmActivityType.Email, CrmActivityDirection.Inbound, "Import externe", null,
            attemptAt, null, metadata);

        Assert.True(result.IsSuccess);
        Assert.Equal(CrmActivityOrigin.Imported, result.Value.Metadata.Origin);
        Assert.Equal("source:42", result.Value.Metadata.IdempotencyKey);
        Assert.Equal(CrmActivitySyncStatus.Failed, result.Value.Metadata.SyncStatus);
        Assert.True(result.Value.Metadata.RequiresRegularization);
    }
}
