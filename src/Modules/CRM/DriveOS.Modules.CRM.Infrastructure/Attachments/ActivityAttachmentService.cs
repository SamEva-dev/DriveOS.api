using DriveOS.Modules.CRM.Application.Activities.Attachments;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DriveOS.Modules.CRM.Infrastructure.Attachments;

internal sealed class ActivityAttachmentService(CrmDbContext db, IConfiguration configuration)
    : IActivityAttachmentService
{
    private const long MaximumLength = 10 * 1024 * 1024;
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    { "application/pdf", "image/jpeg", "image/png", "text/plain" };
    private readonly string root = configuration["Crm:ActivityAttachments:RootPath"]
        ?? Path.Combine(AppContext.BaseDirectory, "storage", "crm-activity-attachments");

    public async Task<Result> UploadAsync(OrganizationId org, CrmActivityId id, string fileName,
        string contentType, long length, Stream content, CancellationToken ct)
    {
        if (length is <= 0 or > MaximumLength || !AllowedTypes.Contains(contentType))
            return Result.Failure(CrmActivityErrors.AttachmentInvalid);
        CrmActivity? activity = await FindAsync(org, id, ct);
        if (activity is null) return Result.Failure(NotFound());
        string safeName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeName) || safeName.Length > 255)
            return Result.Failure(CrmActivityErrors.AttachmentInvalid);
        string reference = Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant();
        string directory = Path.Combine(root, org.Value.ToString("N"), id.Value.ToString("N"));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, reference);
        await using (FileStream output = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            await content.CopyToAsync(output, ct);
        Result domain = activity.SetAttachment(safeName, reference);
        if (domain.IsFailure) { File.Delete(path); return domain; }
        try { await db.SaveChangesAsync(ct); }
        catch { File.Delete(path); throw; }
        return Result.Success();
    }

    public async Task<Result<ActivityAttachmentDownload>> DownloadAsync(OrganizationId org, CrmActivityId id, CancellationToken ct)
    {
        CrmActivity? activity = await db.Activities.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);
        string? reference = activity?.Metadata.AttachmentReference;
        if (activity is null) return Result.Failure<ActivityAttachmentDownload>(NotFound());
        if (string.IsNullOrWhiteSpace(reference)) return Result.Failure<ActivityAttachmentDownload>(CrmActivityErrors.AttachmentNotFound);
        string path = Path.Combine(root, org.Value.ToString("N"), id.Value.ToString("N"), reference);
        if (!File.Exists(path)) return Result.Failure<ActivityAttachmentDownload>(CrmActivityErrors.AttachmentUnavailable);
        return Result.Success(new ActivityAttachmentDownload(
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true),
            activity.Metadata.AttachmentName!, ContentType(activity.Metadata.AttachmentName!)));
    }

    public async Task<Result> DeleteAsync(OrganizationId org, CrmActivityId id, UserId userId, CancellationToken ct)
    {
        CrmActivity? activity = await FindAsync(org, id, ct);
        if (activity is null) return Result.Failure(NotFound());
        string? reference = activity.Metadata.AttachmentReference;
        Result result = activity.RemoveAttachment();
        if (result.IsFailure) return result;
        activity.SetModifiedAudit(DateTimeOffset.UtcNow, userId);
        await db.SaveChangesAsync(ct);
        if (reference is not null)
        {
            string path = Path.Combine(root, org.Value.ToString("N"), id.Value.ToString("N"), reference);
            if (File.Exists(path)) File.Delete(path);
        }
        return Result.Success();
    }

    private Task<CrmActivity?> FindAsync(OrganizationId org, CrmActivityId id, CancellationToken ct) =>
        db.Activities.SingleOrDefaultAsync(x => x.OrganizationId == org && x.Id == id, ct);
    private static Error NotFound() => Error.NotFound("Crm.Activities.NotFound", "errors.crm.activities.notFound");
    private static string ContentType(string name) => Path.GetExtension(name).ToLowerInvariant() switch
    { ".pdf" => "application/pdf", ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".txt" => "text/plain", _ => "application/octet-stream" };
}
