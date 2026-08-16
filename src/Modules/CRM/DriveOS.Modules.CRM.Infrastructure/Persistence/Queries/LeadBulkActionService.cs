using DriveOS.Modules.CRM.Application.Leads.BulkActions;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class LeadBulkActionService(CrmDbContext db) : ILeadBulkActionService
{
    public async Task<LeadBulkActionResult> ExecuteAsync(
        OrganizationId org,
        LeadBulkActionInput input,
        CancellationToken ct
    )
    {
        Guid[] ids = input.LeadIds.Where(x => x != Guid.Empty).Distinct().Take(200).ToArray();
        LeadId[] leadIds = ids.Select(id => new LeadId(id)).ToArray();
        Lead[] leads = await db
            .Leads.Where(x => x.OrganizationId == org && leadIds.Contains(x.Id))
            .ToArrayAsync(ct);
        var byId = leads.ToDictionary(x => x.Id.Value);
        var items = new List<LeadBulkActionItem>(ids.Length);
        foreach (Guid id in ids)
        {
            if (!byId.TryGetValue(id, out Lead? lead))
            {
                items.Add(new(id, false, "Crm.Leads.NotFound"));
                continue;
            }
            var result = input.Action switch
            {
                LeadBulkActionType.AssignAdvisor => lead.AssignAdvisor(
                    input.AdvisorId.HasValue ? new UserId(input.AdvisorId.Value) : null
                ),
                LeadBulkActionType.ChangeStatus when input.TargetStatus.HasValue =>
                    lead.ChangeStatus(input.TargetStatus.Value, input.Reason),
                _ => DriveOS.SharedKernel.Results.Result.Failure(LeadErrors.InvalidStatus),
            };
            items.Add(new(id, result.IsSuccess, result.IsFailure ? result.Error.Code : null));
        }
        if (items.Any(x => x.Succeeded))
            await db.SaveChangesAsync(ct);
        return new(
            ids.Length,
            items.Count(x => x.Succeeded),
            items.Count(x => !x.Succeeded),
            items
        );
    }
}
