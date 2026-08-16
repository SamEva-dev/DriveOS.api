using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CRM.Application.Activities.Manage;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Queries;

internal sealed class ActivityManagementService(CrmDbContext db, IClock clock)
    : IActivityManagementService
{
    public async Task<Result> AttachAsync(
        OrganizationId org,
        CrmActivityId id,
        Guid leadId,
        CancellationToken ct
    )
    {
        if (
            !await db
                .Leads.AsNoTracking()
                .AnyAsync(x => x.OrganizationId == org && x.Id == new LeadId(leadId), ct)
        )
            return Result.Failure(LeadErrors.NotFound);
        return await Mutate(org, id, x => x.AttachToLead(new LeadId(leadId)), ct);
    }

    public Task<Result> InvalidateAsync(
        OrganizationId org,
        CrmActivityId id,
        UserId userId,
        string reason,
        CancellationToken ct
    ) => Mutate(org, id, x => x.Invalidate(reason, userId, clock.UtcNow), ct);

    public Task<Result> RetrySyncAsync(
        OrganizationId org,
        CrmActivityId id,
        CancellationToken ct
    ) => Mutate(org, id, x => x.RetrySynchronization(clock.UtcNow), ct);

    public Task<Result> AbandonSyncAsync(
        OrganizationId org,
        CrmActivityId id,
        CancellationToken ct
    ) => Mutate(org, id, x => x.AbandonSynchronization(clock.UtcNow), ct);

    private async Task<Result> Mutate(
        OrganizationId org,
        CrmActivityId id,
        Func<CrmActivity, Result> mutation,
        CancellationToken ct
    )
    {
        CrmActivity? activity = await db.Activities.SingleOrDefaultAsync(
            x => x.OrganizationId == org && x.Id == id,
            ct
        );
        if (activity is null)
            return Result.Failure(
                Error.NotFound("Crm.Activities.NotFound", "errors.crm.activities.notFound")
            );
        Result result = mutation(activity);
        if (result.IsFailure)
            return result;
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
