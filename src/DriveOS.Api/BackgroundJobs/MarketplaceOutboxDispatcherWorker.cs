using System.Text.Json;
using DomainRelay.Abstractions;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.BackgroundJobs;

internal sealed class MarketplaceOutboxDispatcherWorker(
    IServiceScopeFactory scopes,
    ILogger<MarketplaceOutboxDispatcherWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            bool dispatched = false;
            try
            {
                using IServiceScope scope = scopes.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<ProfessionalMarketplaceDbContext>();
                Guid[] candidates = await database.OutboxMessages.AsNoTracking()
                    .Where(x => x.Status != "Processed" && x.Status != "Dead" && x.NextAttemptAtUtc <= DateTimeOffset.UtcNow)
                    .OrderBy(x => x.OccurredAtUtc)
                    .Select(x => x.Id)
                    .Take(50)
                    .ToArrayAsync(stoppingToken);

                foreach (Guid id in candidates)
                {
                    dispatched |= await DispatchOneAsync(scope.ServiceProvider, database, id, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Marketplace outbox polling failed.");
            }

            if (!dispatched) await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task<bool> DispatchOneAsync(
        IServiceProvider services,
        ProfessionalMarketplaceDbContext database,
        Guid id,
        CancellationToken cancellationToken)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        int leased = await database.OutboxMessages
            .Where(x => x.Id == id && x.Status != "Processed" && x.Status != "Dead" && x.NextAttemptAtUtc <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, "Processing")
                .SetProperty(x => x.NextAttemptAtUtc, now.AddMinutes(5))
                .SetProperty(x => x.AttemptCount, x => x.AttemptCount + 1), cancellationToken);
        if (leased == 0) return false;

        database.ChangeTracker.Clear();
        MarketplaceOutboxMessage message = await database.OutboxMessages.SingleAsync(x => x.Id == id, cancellationToken);
        try
        {
            Type? eventType = Type.GetType(message.EventType, throwOnError: false);
            if (eventType?.Namespace?.StartsWith("DriveOS.Modules.ProfessionalMarketplace.", StringComparison.Ordinal) != true)
                throw new InvalidOperationException("OUTBOX_EVENT_TYPE_NOT_ALLOWED");
            object domainEvent = JsonSerializer.Deserialize(message.PayloadJson, eventType, JsonOptions)
                ?? throw new InvalidOperationException("OUTBOX_EVENT_DESERIALIZATION_FAILED");
            var mediator = services.GetRequiredService<IMediator>();
            await mediator.Publish((dynamic)domainEvent, cancellationToken);
            message.Status = "Processed";
            message.ProcessedAtUtc = DateTimeOffset.UtcNow;
            message.LastErrorCode = null;
        }
        catch (Exception exception)
        {
            message.Status = message.AttemptCount >= 12 ? "Dead" : "Failed";
            int delaySeconds = Math.Min(900, 1 << Math.Min(message.AttemptCount, 9));
            message.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddSeconds(delaySeconds);
            message.LastErrorCode = exception.GetBaseException().GetType().Name;
            logger.LogWarning(exception, "Marketplace outbox event {EventId} dispatch failed on attempt {Attempt}.", message.EventId, message.AttemptCount);
        }
        await database.SaveChangesAsync(cancellationToken);
        return true;
    }
}
