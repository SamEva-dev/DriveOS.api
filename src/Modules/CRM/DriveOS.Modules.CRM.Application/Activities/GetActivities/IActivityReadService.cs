using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CRM.Application.Activities.GetActivities;

public sealed record ActivityListQuery(int PageNumber, int PageSize, string? Search,
    CrmActivityType? Type, UserId? AdvisorUserId, Guid? LeadId, bool UnattachedOnly,
    bool ImportedOnly, bool SyncErrorsOnly, bool DuplicatesOnly, bool RegularizationOnly,
    bool UnfollowedOnly, CrmActivityReadScope Scope, DateTimeOffset? FromUtc, DateTimeOffset? ToUtc);
public sealed record ActivityListItem(Guid Id, Guid? LeadId, string? LeadName, string Type,
    string Direction, string Subject, string? Details, DateTimeOffset OccurredAtUtc,
    Guid? AdvisorUserId, string? AdvisorName, string? Result, int? DurationMinutes,
    bool IsInternal, bool IsUnfollowed, bool RequiresRegularization, string Origin,
    string SyncStatus, string? SyncErrorKey, int SyncAttemptCount, bool HasPotentialDuplicate,
    string? AttachmentName, string? AttachmentReference, bool IsInvalidated);
public sealed record ActivityPage(IReadOnlyList<ActivityListItem> Items, int PageNumber,
    int PageSize, int TotalCount, int TotalPages);

public interface IActivityReadService
{
    Task<ActivityPage> GetPageAsync(OrganizationId organizationId, ActivityListQuery query,
        CancellationToken cancellationToken);
}
