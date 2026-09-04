using System.Runtime.CompilerServices;
using System.Text.Json;
using DriveOS.SharedKernel.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Outbox;

internal sealed class MarketplaceOutboxInterceptor : SaveChangesInterceptor
{
    private static readonly ConditionalWeakTable<DbContext, IReadOnlyCollection<IHasDomainEvents>> Pending = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Clear(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Clear(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null) Pending.Remove(eventData.Context);
        base.SaveChangesFailed(eventData);
    }

    private static void Capture(DbContext? context)
    {
        if (context is null) return;
        IHasDomainEvents[] aggregates = context.ChangeTracker.Entries()
            .Select(x => x.Entity)
            .OfType<IHasDomainEvents>()
            .Where(x => x.DomainEvents.Count > 0)
            .ToArray();
        if (aggregates.Length == 0) return;

        HashSet<Guid> tracked = context.ChangeTracker.Entries<MarketplaceOutboxMessage>()
            .Select(x => x.Entity.EventId)
            .ToHashSet();
        foreach (IDomainEvent domainEvent in aggregates.SelectMany(x => x.DomainEvents))
        {
            if (!tracked.Add(domainEvent.EventId)) continue;
            Type type = domainEvent.GetType();
            context.Set<MarketplaceOutboxMessage>().Add(new MarketplaceOutboxMessage
            {
                Id = Guid.NewGuid(),
                EventId = domainEvent.EventId,
                EventType = type.AssemblyQualifiedName ?? type.FullName ?? type.Name,
                PayloadJson = JsonSerializer.Serialize(domainEvent, type, JsonOptions),
                OccurredAtUtc = domainEvent.OccurredAtUtc,
                EnqueuedAtUtc = DateTimeOffset.UtcNow,
                NextAttemptAtUtc = DateTimeOffset.UtcNow
            });
        }
        Pending.Remove(context);
        Pending.Add(context, aggregates);
    }

    private static void Clear(DbContext? context)
    {
        if (context is null || !Pending.TryGetValue(context, out IReadOnlyCollection<IHasDomainEvents>? aggregates)) return;
        foreach (IHasDomainEvents aggregate in aggregates) aggregate.ClearDomainEvents();
        Pending.Remove(context);
    }
}
