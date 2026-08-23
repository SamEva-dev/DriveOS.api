using System.Text.Json;
using DriveOS.Application.Abstractions.Integrations.RegulatoryTrainingRecords;
using DriveOS.Modules.RegulatoryIntegrations.Application.Reconciliation;
using DriveOS.Modules.RegulatoryIntegrations.Application.Submissions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.RegulatoryIntegrations.Infrastructure.Reconciliation;

internal sealed class RegulatoryTrainingRecordReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<RegulatoryTrainingRecordReconciliationOptions> options,
    ILogger<RegulatoryTrainingRecordReconciliationWorker> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled) return;

        TimeSpan delay = TimeSpan.FromSeconds(Math.Clamp(settings.PollSeconds, 30, 3600));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var store = scope.ServiceProvider.GetRequiredService<IRegulatoryTrainingRecordReconciliationStore>();
                var projector = scope.ServiceProvider.GetRequiredService<IRegulatoryTrainingSessionProjector>();
                var submissions = scope.ServiceProvider.GetRequiredService<IRegulatoryTrainingRecordSubmissionService>();

                IReadOnlyList<RegulatoryTrainingRecordReconciliationCandidate> candidates =
                    await store.GetCandidatesAsync(settings.BatchSize, stoppingToken);

                foreach (var candidate in candidates)
                {
                    RegulatoryTrainingSessionProjection? previous = JsonSerializer.Deserialize<RegulatoryTrainingSessionProjection>(candidate.PayloadJson, JsonOptions);
                    if (previous is null) continue;

                    var source = new RegulatoryTrainingSessionProjectionSource(
                        previous.OrganizationId, previous.StudentOwnerOrganizationId, previous.PerformingOrganizationId,
                        previous.SessionId, previous.StudentId, previous.TrainingPathId, previous.InstructorId, previous.BranchId,
                        previous.VehicleId, previous.TrainingCategory, previous.ActualStartAtUtc, previous.ActualEndAtUtc,
                        previous.DeliveredDurationMinutes, previous.CompletedAtUtc);

                    var projected = await projector.ProjectAsync(source, stoppingToken);
                    if (projected.IsFailure) continue;
                    await submissions.ReconcileAsync(projected.Value, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Regulatory training record reconciliation cycle failed.");
            }

            await Task.Delay(delay, stoppingToken);
        }
    }
}
