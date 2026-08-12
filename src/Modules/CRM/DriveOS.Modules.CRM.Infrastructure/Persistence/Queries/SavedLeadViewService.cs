using DriveOS.Modules.CRM.Application.Leads.SavedViews;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using DriveOS.Application.Abstractions.Time;
using System.Text.Json;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class SavedLeadViewService(CrmDbContext db, IClock clock) : ISavedLeadViewService
{
    public async Task<IReadOnlyList<SavedLeadViewDto>> ListAsync(OrganizationId org, UserId user,
        IReadOnlySet<Guid> branchIds, CancellationToken ct) => await db.SavedLeadViews.AsNoTracking()
        .Where(x => x.OrganizationId == org && (x.OwnerUserId == user ||
            x.Scope == SavedLeadViewScope.Organization ||
            (x.Scope == SavedLeadViewScope.Branch && x.BranchId.HasValue && branchIds.Contains(x.BranchId.Value))))
        .OrderByDescending(x => x.IsDefault && x.OwnerUserId == user).ThenBy(x => x.Name)
        .Select(x => new SavedLeadViewDto(x.Id, x.Name, x.FiltersJson, x.SortJson, x.ColumnsJson,
            x.Scope, x.BranchId, x.IsDefault, x.OwnerUserId.Value, x.OwnerUserId == user))
        .ToArrayAsync(ct);

    public async Task<SavedLeadViewDto?> SaveAsync(OrganizationId org, UserId user,
        SaveLeadViewInput input, bool canShare, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name) || input.Name.Trim().Length > 120) return null;
        if (input.FiltersJson.Length > 20_000 || input.SortJson.Length > 2_000 ||
            input.ColumnsJson.Length > 10_000 || !IsJson(input.FiltersJson) ||
            !IsJson(input.SortJson) || !IsJson(input.ColumnsJson)) return null;
        if (!canShare && input.Scope != SavedLeadViewScope.Private) return null;
        if (input.Scope == SavedLeadViewScope.Branch && input.BranchId is null) return null;
        DateTimeOffset now = clock.UtcNow;
        SavedLeadView? view = input.Id.HasValue
            ? await db.SavedLeadViews.SingleOrDefaultAsync(x => x.Id == input.Id &&
                x.OrganizationId == org && x.OwnerUserId == user, ct) : null;
        if (input.Id.HasValue && view is null) return null;
        if (input.IsDefault)
        {
            SavedLeadView[] defaults = await db.SavedLeadViews.Where(x => x.OrganizationId == org &&
                x.OwnerUserId == user && x.IsDefault).ToArrayAsync(ct);
            foreach (SavedLeadView item in defaults) item.ClearDefault();
        }
        if (view is null)
        {
            view = new SavedLeadView(Guid.NewGuid(), org, user, input.Name, input.FiltersJson,
                input.SortJson, input.ColumnsJson, input.Scope, input.BranchId, input.IsDefault, now);
            await db.SavedLeadViews.AddAsync(view, ct);
        }
        else view.Update(input.Name, input.FiltersJson, input.SortJson, input.ColumnsJson,
            input.Scope, input.BranchId, input.IsDefault, now);
        await db.SaveChangesAsync(ct);
        return new(view.Id, view.Name, view.FiltersJson, view.SortJson, view.ColumnsJson,
            view.Scope, view.BranchId, view.IsDefault, view.OwnerUserId.Value, true);
    }

    public async Task<bool> DeleteAsync(OrganizationId org, UserId user, Guid id, CancellationToken ct)
    {
        SavedLeadView? view = await db.SavedLeadViews.SingleOrDefaultAsync(x => x.Id == id &&
            x.OrganizationId == org && x.OwnerUserId == user, ct);
        if (view is null) return false;
        db.SavedLeadViews.Remove(view); await db.SaveChangesAsync(ct); return true;
    }

    private static bool IsJson(string value)
    {
        try { using JsonDocument _ = JsonDocument.Parse(value); return true; }
        catch (JsonException) { return false; }
    }
}
