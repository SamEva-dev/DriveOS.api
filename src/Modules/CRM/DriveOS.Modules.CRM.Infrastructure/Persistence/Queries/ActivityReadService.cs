using DriveOS.Modules.CRM.Application.Activities.GetActivities;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class ActivityReadService(CrmDbContext db) : IActivityReadService
{
    public async Task<ActivityPage> GetPageAsync(OrganizationId org, ActivityListQuery f, CancellationToken ct)
    {
        int page = Math.Max(1, f.PageNumber), size = Math.Clamp(f.PageSize, 10, 100);
        IQueryable<CrmActivity> q = db.Activities.AsNoTracking()
            .Where(x => x.OrganizationId == org && x.InvalidatedAtUtc == null);
        if (f.Scope == CrmActivityReadScope.PublicOnly) q = q.Where(x => !x.Metadata.IsInternal);
        if (f.Type.HasValue) q = q.Where(x => x.Type == f.Type);
        if (f.AdvisorUserId.HasValue) q = q.Where(x => x.AdvisorUserId == f.AdvisorUserId);
        if (f.LeadId.HasValue) { var id = new LeadId(f.LeadId.Value); q = q.Where(x => x.LeadId == id); }
        if (f.UnattachedOnly) q = q.Where(x => x.LeadId == null);
        if (f.ImportedOnly) q = q.Where(x => x.Metadata.Origin == CrmActivityOrigin.Imported);
        if (f.SyncErrorsOnly) q = q.Where(x => x.Metadata.SyncStatus == CrmActivitySyncStatus.Failed);
        if (f.DuplicatesOnly)
        {
            IQueryable<string> duplicateKeyQuery = db.Activities.AsNoTracking()
                .Where(x => x.OrganizationId == org && x.Metadata.IdempotencyKey != null)
                .GroupBy(x => x.Metadata.IdempotencyKey!).Where(g => g.Count() > 1).Select(g => g.Key);
            q = q.Where(x => x.Metadata.IdempotencyKey != null && duplicateKeyQuery.Contains(x.Metadata.IdempotencyKey));
        }
        if (f.RegularizationOnly) q = q.Where(x => x.Metadata.RequiresRegularization);
        if (f.UnfollowedOnly) q = q.Where(x => x.Metadata.IsUnfollowed);
        if (f.FromUtc.HasValue) q = q.Where(x => x.OccurredAtUtc >= f.FromUtc);
        if (f.ToUtc.HasValue) q = q.Where(x => x.OccurredAtUtc <= f.ToUtc);
        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            string search = f.Search.Trim().ToLower();
            q = q.Where(x => x.Subject.ToLower().Contains(search) ||
                (x.Details != null && x.Details.ToLower().Contains(search)));
        }
        int total = await q.CountAsync(ct);
        var rows = await (
            from activity in q
                .OrderByDescending(x => x.OccurredAtUtc)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Skip((page - 1) * size)
                .Take(size)
            join lead in db.Leads.AsNoTracking().Where(x => x.OrganizationId == org)
                on activity.LeadId equals (LeadId?)lead.Id into activityLeads
            from lead in activityLeads.DefaultIfEmpty()
            select new
            {
                Activity = activity,
                LeadGuid = activity.LeadId.HasValue ? activity.LeadId.Value.Value : (Guid?)null,
                LeadFirstName = lead == null ? null : lead.Identity.FirstName,
                LeadLastName = lead == null ? null : lead.Identity.LastName
            })
            .ToArrayAsync(ct);
        string[] keys = rows.Select(x => x.Activity.Metadata.IdempotencyKey).Where(x => x != null).Cast<string>().ToArray();
        var duplicateKeys = await db.Activities.AsNoTracking().Where(x => x.OrganizationId == org &&
            x.Metadata.IdempotencyKey != null && keys.Contains(x.Metadata.IdempotencyKey))
            .GroupBy(x => x.Metadata.IdempotencyKey!).Where(g => g.Count() > 1).Select(g => g.Key).ToArrayAsync(ct);
        var duplicateSet = duplicateKeys.ToHashSet();
        ActivityListItem[] items = rows.Select(x => {
            var a = x.Activity; string? leadName = x.LeadGuid.HasValue ? $"{x.LeadFirstName} {x.LeadLastName}".Trim() : null;
            return new ActivityListItem(a.Id.Value, x.LeadGuid, leadName, a.Type.ToString(), a.Direction.ToString(),
                a.Subject, a.Details, a.OccurredAtUtc, a.AdvisorUserId?.Value, a.AdvisorUserId?.ToString(),
                a.Metadata.Result, a.Metadata.DurationMinutes, a.Metadata.IsInternal, a.Metadata.IsUnfollowed,
                a.Metadata.RequiresRegularization, a.Metadata.Origin.ToString(), a.Metadata.SyncStatus.ToString(),
                a.Metadata.SyncErrorKey, a.Metadata.SyncAttemptCount,
                a.Metadata.IdempotencyKey != null && duplicateSet.Contains(a.Metadata.IdempotencyKey),
                a.Metadata.AttachmentName, a.Metadata.AttachmentReference, a.InvalidatedAtUtc.HasValue);
        }).ToArray();
        return new(items, page, size, total, (int)Math.Ceiling(total / (double)size));
    }
}
