using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Application.Success;
using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DriveOS.Modules.ExamsCertification.Infrastructure.Configuration;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Success;

internal sealed class ExamSuccessConsequenceWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<ExamSuccessConsequenceWorker> logger,
    IClock clock,
    IOptions<ExamSuccessConsequencesOptions> options) : BackgroundService
{
    private readonly ExamSuccessConsequencesOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Exam success consequence worker is disabled by configuration.");
            return;
        }

        TimeSpan processingLease = TimeSpan.FromMinutes(_options.ProcessingLeaseMinutes);
        TimeSpan pollInterval = TimeSpan.FromSeconds(_options.PollSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IExamSuccessConsequenceStore>();
                var gateway = scope.ServiceProvider.GetRequiredService<IExamSuccessConsequenceGateway>();
                var processes = scope.ServiceProvider.GetRequiredService<IExamSuccessProcessRepository>();
                var uow = scope.ServiceProvider.GetRequiredService<IExamsCertificationUnitOfWork>();
                IReadOnlyList<ExamSuccessConsequenceEnvelope> due = await store.ClaimDueAsync(_options.BatchSize, clock.UtcNow, processingLease, stoppingToken);
                foreach (ExamSuccessConsequenceEnvelope item in due)
                {
                    DateTimeOffset attemptedAt = clock.UtcNow;
                    try
                    {
                        await SyncAsync(processes, uow, item, ExamSuccessActionStatus.Processing, null, null, attemptedAt, stoppingToken);
                        ExamSuccessConsequenceDispatchResult result = await gateway.DispatchAsync(item, stoppingToken);
                        if (result.IsProcessed)
                        {
                            await store.MarkProcessedAsync(item.Id, attemptedAt, stoppingToken);
                            await SyncAsync(processes, uow, item, ExamSuccessActionStatus.Completed, null, result.Detail, attemptedAt, stoppingToken);
                        }
                        else if (result.IsDeferred)
                        {
                            await store.MarkDeferredAsync(item.Id, result.Code ?? "integration.deferred", result.Detail, attemptedAt, attemptedAt.AddHours(_options.DeferredRetryHours), stoppingToken);
                            await SyncAsync(processes, uow, item, ExamSuccessActionStatus.Deferred, result.Code, result.Detail, attemptedAt, stoppingToken);
                        }
                        else
                        {
                            await store.MarkFailedAsync(item.Id, result.Code ?? "integration.failed", result.Detail, result.IsPermanentFailure, attemptedAt,
                                result.IsPermanentFailure ? null : attemptedAt.AddMinutes(Math.Min(_options.MaxRetryMinutes, 2 << Math.Min(item.AttemptCount, 5))), stoppingToken);
                            await SyncAsync(processes, uow, item, result.IsPermanentFailure ? ExamSuccessActionStatus.Blocked : ExamSuccessActionStatus.Failed,
                                result.Code, result.Detail, attemptedAt, stoppingToken);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Exam success consequence {ConsequenceId} failed.", item.Id);
                        await store.MarkFailedAsync(item.Id, "integration.exception", ex.GetType().Name, false, attemptedAt, attemptedAt.AddMinutes(_options.ExceptionRetryMinutes), stoppingToken);
                        try { await SyncAsync(processes, uow, item, ExamSuccessActionStatus.Failed, "integration.exception", ex.GetType().Name, attemptedAt, stoppingToken); }
                        catch (Exception syncEx) { logger.LogError(syncEx, "Exam success process synchronization failed for {ConsequenceId}.", item.Id); }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
            catch (Exception ex) { logger.LogError(ex, "Exam success consequence dispatcher cycle failed."); }
            try { await Task.Delay(pollInterval, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
        }
    }

    private static async Task SyncAsync(IExamSuccessProcessRepository repository, IExamsCertificationUnitOfWork uow,
        ExamSuccessConsequenceEnvelope item, ExamSuccessActionStatus status, string? reasonCode, string? detail, DateTimeOffset now, CancellationToken ct)
    {
        ExamSuccessProcess? process = await repository.GetByResultForUpdateAsync(item.OrganizationId, item.ResultId, item.Snapshot.ResultRevision, ct);
        if (process is null || process.Status == ExamSuccessProcessStatus.Superseded) return;
        ExamSuccessActionCode? actionCode = Map(item.Kind);
        if (!actionCode.HasValue) return;
        process.ApplyConsequence(actionCode.Value, status, $"consequence:{item.Id:N}", reasonCode, detail, null, now);
        await uow.CommitAsync(ct);
    }

    private static ExamSuccessActionCode? Map(ExamSuccessConsequenceKind kind) => kind switch
    {
        ExamSuccessConsequenceKind.PedagogicalCompletion => ExamSuccessActionCode.ClosePedagogicalPath,
        ExamSuccessConsequenceKind.StudentJourneyTransition => ExamSuccessActionCode.UpdateStudentJourney,
        ExamSuccessConsequenceKind.ContractCompletion => ExamSuccessActionCode.CloseTrainingContract,
        ExamSuccessConsequenceKind.FinancialClosureReview => ExamSuccessActionCode.CheckFinancialSituation,
        ExamSuccessConsequenceKind.CertificationEligibility => ExamSuccessActionCode.PrepareCertification,
        ExamSuccessConsequenceKind.SchedulingFollowUpReview => ExamSuccessActionCode.ReviewFutureScheduling,
        ExamSuccessConsequenceKind.SuccessCommunication => ExamSuccessActionCode.NotifyStudent,
        ExamSuccessConsequenceKind.AnalyticsMetrics => ExamSuccessActionCode.PublishAnalytics,
        _ => null
    };
}
