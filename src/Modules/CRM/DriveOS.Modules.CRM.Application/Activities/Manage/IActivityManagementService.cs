using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Activities.Manage;

public interface IActivityManagementService
{
    Task<Result> AttachAsync(
        OrganizationId organizationId,
        CrmActivityId activityId,
        Guid leadId,
        CancellationToken ct
    );
    Task<Result> InvalidateAsync(
        OrganizationId organizationId,
        CrmActivityId activityId,
        UserId userId,
        string reason,
        CancellationToken ct
    );
    Task<Result> RetrySyncAsync(
        OrganizationId organizationId,
        CrmActivityId activityId,
        CancellationToken ct
    );
    Task<Result> AbandonSyncAsync(
        OrganizationId organizationId,
        CrmActivityId activityId,
        CancellationToken ct
    );
}
