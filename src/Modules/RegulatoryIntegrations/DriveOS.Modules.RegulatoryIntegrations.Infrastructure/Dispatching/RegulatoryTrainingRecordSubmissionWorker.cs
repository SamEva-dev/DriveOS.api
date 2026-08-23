using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.RegulatoryIntegrations.Application.Dispatching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Dispatching;

internal sealed class RegulatoryTrainingRecordSubmissionWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<RegulatoryTrainingRecordSubmissionWorker> logger,
    IClock clock,
    IOptions<RegulatoryTrainingRecordDispatchOptions> options) : BackgroundService
{
    private readonly RegulatoryTrainingRecordDispatchOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Regulatory training record dispatcher is disabled by configuration.");
            return;
        }

        TimeSpan pollInterval = TimeSpan.FromSeconds(Math.Clamp(_options.PollSeconds, 1, 300));
        TimeSpan processingLease = TimeSpan.FromMinutes(Math.Clamp(_options.ProcessingLeaseMinutes, 1, 60));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IRegulatoryTrainingRecordSubmissionDispatchStore>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IRegulatoryTrainingRecordTransportDispatcher>();

                IReadOnlyList<RegulatoryTrainingRecordSubmissionDispatchEnvelope> due = await store.ClaimDueAsync(
                    Math.Clamp(_options.BatchSize, 1, 500),
                    clock.UtcNow,
                    processingLease,
                    stoppingToken);

                foreach (RegulatoryTrainingRecordSubmissionDispatchEnvelope envelope in due)
                {
                    DateTimeOffset attemptedAtUtc = clock.UtcNow;
                    try
                    {
                        var request = new RegulatoryTrainingRecordTransportRequest(
                            envelope.SubmissionId,
                            envelope.ProjectionId,
                            envelope.ProjectionSchemaVersion,
                            envelope.OrganizationId,
                            envelope.TrainingSessionId,
                            envelope.CountryCode,
                            envelope.ProviderCode,
                            envelope.PayloadJson,
                            envelope.PayloadHash,
                            envelope.AttemptNumber);

                        RegulatoryTrainingRecordTransportResult result = await dispatcher.DispatchAsync(request, stoppingToken);
                        DateTimeOffset? nextAttempt = ResolveNextAttempt(result, envelope.AttemptNumber, attemptedAtUtc);

                        await store.ApplyResultAsync(
                            envelope.SubmissionId,
                            result,
                            attemptedAtUtc,
                            nextAttempt,
                            stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Regulatory training record submission {SubmissionId} dispatch failed.", envelope.SubmissionId);

                        // The lease makes this item recoverable even if persisting the failure itself fails.
                        try
                        {
                            await store.ApplyResultAsync(
                                envelope.SubmissionId,
                                RegulatoryTrainingRecordTransportResult.Retry(
                                    "transport-exception",
                                    ex.GetType().Name,
                                    TimeSpan.FromMinutes(Math.Clamp(_options.DefaultRetryMinutes, 1, 1440))),
                                attemptedAtUtc,
                                attemptedAtUtc.AddMinutes(Math.Clamp(_options.DefaultRetryMinutes, 1, 1440)),
                                stoppingToken);
                        }
                        catch (Exception persistenceEx)
                        {
                            logger.LogError(persistenceEx, "Could not persist failure for regulatory submission {SubmissionId}.", envelope.SubmissionId);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Regulatory training record dispatcher cycle failed.");
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

    private DateTimeOffset? ResolveNextAttempt(
        RegulatoryTrainingRecordTransportResult result,
        int attemptNumber,
        DateTimeOffset attemptedAtUtc)
    {
        if (result.Outcome is RegulatoryTrainingRecordTransportOutcome.Accepted or RegulatoryTrainingRecordTransportOutcome.Rejected)
            return null;

        if (result.RetryAfter is TimeSpan providerDelay && providerDelay > TimeSpan.Zero)
            return attemptedAtUtc.Add(providerDelay);

        int minutes = result.Outcome == RegulatoryTrainingRecordTransportOutcome.Unavailable
            ? Math.Clamp(_options.UnavailableRetryMinutes, 1, 1440)
            : Math.Min(
                Math.Clamp(_options.MaxRetryMinutes, 1, 1440),
                Math.Max(1, _options.DefaultRetryMinutes) * (1 << Math.Min(Math.Max(attemptNumber - 1, 0), 5)));

        return attemptedAtUtc.AddMinutes(minutes);
    }
}
