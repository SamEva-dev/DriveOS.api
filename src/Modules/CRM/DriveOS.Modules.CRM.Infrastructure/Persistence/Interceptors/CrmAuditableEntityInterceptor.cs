using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Time;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DriveOS.Modules.CRM.Infrastructure.Persistence.Interceptors;

internal sealed class CrmAuditableEntityInterceptor(IClock clock, ICurrentUser currentUser)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        DateTimeOffset now = clock.UtcNow;
        UserId? currentUserId = currentUser.UserId;

        foreach (var entry in dbContext.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAudit(now, currentUserId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetModifiedAudit(now, currentUserId);
            }
        }
    }
}
