using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Places.Watch;
using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Places.Watch;

/// <summary>
/// Polls due exam-place watch subscriptions. Provider-specific throttling and credentials stay inside each provider;
/// this worker only schedules tenant-scoped watch commands and uses a database lease for multi-instance deployments.
/// </summary>
internal sealed class ExamPlaceWatchWorker(
    IServiceScopeFactory scopeFactory,
    IClock clock,
    IOptions<ExamPlaceWatcherOptions> options,
    ILogger<ExamPlaceWatchWorker> logger) : BackgroundService
{
    private readonly ExamPlaceWatcherOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Exam place watcher is disabled by configuration.");
            return;
        }

        TimeSpan processingLease = TimeSpan.FromMinutes(_options.ProcessingLeaseMinutes);
        TimeSpan pollInterval = TimeSpan.FromSeconds(_options.PollSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                IExamPlaceWatchRepository repository = scope.ServiceProvider.GetRequiredService<IExamPlaceWatchRepository>();
                IMediator mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                IReadOnlyList<ExamPlaceWatchSubscription> due = await repository.ClaimDueAsync(clock.UtcNow, _options.BatchSize, processingLease, stoppingToken);
                foreach (ExamPlaceWatchSubscription subscription in due)
                {
                    if (subscription.CreatedByUserId is not { } actorUserId)
                    {
                        logger.LogWarning("Exam place watch {SubscriptionId} has no audit actor and was skipped.", subscription.Id.Value);
                        continue;
                    }

                    try
                    {
                        await mediator.Send(new RunExamPlaceWatchCommand(subscription.OrganizationId, subscription.Id, actorUserId), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exam place watch {SubscriptionId} failed unexpectedly.", subscription.Id.Value);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exam place watcher cycle failed.");
            }

            try
            {
                await Task.Delay(pollInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
