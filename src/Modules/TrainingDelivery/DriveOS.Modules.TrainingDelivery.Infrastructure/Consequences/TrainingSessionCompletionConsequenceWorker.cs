using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Consequences;

internal sealed class TrainingSessionCompletionConsequenceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TrainingSessionCompletionConsequenceWorker> logger,
    IClock clock) : BackgroundService
{
    private static readonly TimeSpan ProcessingLease = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();

                ITrainingSessionCompletionConsequenceStore store =
                    scope.ServiceProvider.GetRequiredService<ITrainingSessionCompletionConsequenceStore>();

                ITrainingSessionCompletionConsequenceGateway gateway =
                    scope.ServiceProvider.GetRequiredService<ITrainingSessionCompletionConsequenceGateway>();

                DateTimeOffset now = clock.UtcNow;

                IReadOnlyList<TrainingSessionConsequenceEnvelope> due =
                    await store.ClaimDueAsync(50, now, ProcessingLease, stoppingToken);

                foreach (TrainingSessionConsequenceEnvelope item in due)
                {
                    try
                    {
                        TrainingSessionConsequenceDispatchResult result =
                            await gateway.DispatchAsync(item, stoppingToken);

                        DateTimeOffset attemptedAtUtc = clock.UtcNow;

                        if (result.IsProcessed)
                        {
                            await store.MarkProcessedAsync(item.Id, attemptedAtUtc, stoppingToken);
                        }
                        else if (result.IsDeferred)
                        {
                            await store.MarkDeferredAsync(
                                item.Id,
                                result.Code ?? "integration.deferred",
                                result.Detail,
                                attemptedAtUtc,
                                attemptedAtUtc.AddHours(6),
                                stoppingToken);
                        }
                        else
                        {
                            await store.MarkFailedAsync(
                                item.Id,
                                result.Code ?? "integration.failed",
                                result.Detail,
                                result.IsPermanentFailure,
                                attemptedAtUtc,
                                result.IsPermanentFailure
                                    ? null
                                    : attemptedAtUtc.AddMinutes(
                                        Math.Min(60, 2 << Math.Min(item.AttemptCount, 5))),
                                stoppingToken);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(
                            ex,
                            "Training Delivery consequence {ConsequenceId} failed.",
                            item.Id);

                        DateTimeOffset attemptedAtUtc = clock.UtcNow;

                        await store.MarkFailedAsync(
                            item.Id,
                            "integration.exception",
                            ex.GetType().Name,
                            false,
                            attemptedAtUtc,
                            attemptedAtUtc.AddMinutes(5),
                            stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Training Delivery consequence dispatcher cycle failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
