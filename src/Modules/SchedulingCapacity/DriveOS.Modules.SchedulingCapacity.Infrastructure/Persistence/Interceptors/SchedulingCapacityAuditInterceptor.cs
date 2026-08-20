using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Time;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence.Interceptors;

internal sealed class SchedulingCapacityAuditInterceptor(IClock clock, ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext? context)
    {
        if (context is null) return;
        DateTimeOffset now = clock.UtcNow;
        UserId? userId = currentUser.UserId;
        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAtUtc == default)
                entry.Entity.SetCreatedAudit(now, userId);
            else if (entry.State == EntityState.Modified)
                entry.Entity.SetModifiedAudit(now, userId);
        }
    }
}
