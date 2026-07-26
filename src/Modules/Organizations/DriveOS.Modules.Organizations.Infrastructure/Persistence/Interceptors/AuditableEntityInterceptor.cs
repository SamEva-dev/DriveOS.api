using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Time;
using DriveOS.SharedKernel.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DriveOS.Modules.Organizations.Infrastructure
    .Persistence.Interceptors;

internal sealed class AuditableEntityInterceptor :
    SaveChangesInterceptor
{
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public AuditableEntityInterceptor(
        IClock clock,
        ICurrentUser currentUser)
    {
        _clock = clock;
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>>
        SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);

        return base.SavingChangesAsync(
            eventData,
            result,
            cancellationToken);
    }

    private void ApplyAudit(DbContext? dbContext)
    {
        if (dbContext is null)
        {
            return;
        }

        DateTimeOffset now = _clock.UtcNow;
        var currentUserId = _currentUser.UserId;

        foreach (var entry in dbContext.ChangeTracker
                     .Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreatedAudit(
                    now,
                    currentUserId);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetModifiedAudit(
                    now,
                    currentUserId);
            }
        }
    }
}