using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Application.Activities.Attachments;

public sealed record ActivityAttachmentDownload(Stream Content, string FileName, string ContentType);

public interface IActivityAttachmentService
{
    Task<Result> UploadAsync(OrganizationId organizationId, CrmActivityId activityId,
        string fileName, string contentType, long length, Stream content, CancellationToken ct);
    Task<Result<ActivityAttachmentDownload>> DownloadAsync(OrganizationId organizationId,
        CrmActivityId activityId, CancellationToken ct);
    Task<Result> DeleteAsync(OrganizationId organizationId, CrmActivityId activityId,
        UserId userId, CancellationToken ct);
}
