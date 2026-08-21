using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.CancellationConsequences;

internal sealed class TrainingSessionCancellationConsequenceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<TrainingSessionCancellationConsequenceWorker> logger,
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

                ITrainingSessionCancellationConsequenceStore store =
                    scope.ServiceProvider.GetRequiredService<ITrainingSessionCancellationConsequenceStore>();

                ITrainingSessionCancellationConsequenceGateway gateway =
                    scope.ServiceProvider.GetRequiredService<ITrainingSessionCancellationConsequenceGateway>();

                DateTimeOffset now = clock.UtcNow;

                IReadOnlyList<TrainingSessionCancellationConsequenceEnvelope> due =
                    await store.ClaimDueAsync(50, now, ProcessingLease, stoppingToken);

                foreach (TrainingSessionCancellationConsequenceEnvelope item in due)
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
                                    : attemptedAtUtc.AddMinutes(5),
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
                            "Training Delivery cancellation consequence {ConsequenceId} failed.",
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
                logger.LogError(
                    ex,
                    "Training Delivery cancellation consequence dispatcher cycle failed.");
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
