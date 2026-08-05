using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationClosures.Commands;
using DriveOS.Modules.Organizations.Domain.OrganizationClosures;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

internal sealed class OrganizationClosureScheduler(
    OrganizationsDbContext dbContext,
    IOrganizationClosureOrchestrator orchestrator,
    IOrganizationAnonymizationService anonymizationService,
    IOrganizationClosureAuditSink auditSink,
    IClock clock,
    ILogger<OrganizationClosureScheduler> logger)
    : IOrganizationClosureScheduler
{
    private const int BatchSize = 50;

    public async Task<int> ProcessDueClosuresAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.UtcNow;
        List<OrganizationClosure> due = await dbContext.OrganizationClosures
            .Where(x => x.Status == OrganizationClosureStatus.Scheduled &&
                        x.RequestedEffectiveAtUtc <= now)
            .OrderBy(x => x.RequestedEffectiveAtUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        int completed = 0;
        foreach (OrganizationClosure closure in due)
        {
            OrganizationClosureExecutionResult execution = await orchestrator.ExecuteAsync(
                closure,
                closure.RequestedByUserId,
                cancellationToken);

            if (!execution.Succeeded)
            {
                logger.LogWarning("Scheduled organization closure {ClosureId} did not complete.", closure.Id);
                continue;
            }

            var completion = closure.Complete(closure.RequestedByUserId, now);
            if (completion.IsFailure)
            {
                logger.LogWarning("Unable to mark organization closure {ClosureId} completed: {ErrorCode}.", closure.Id, completion.Error.Code);
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await auditSink.WriteAsync(
                "OrganizationClosureCompleted",
                closure.OrganizationId,
                closure.Id,
                closure.RequestedByUserId,
                new Dictionary<string, object?> { ["completedAtUtc"] = now },
                cancellationToken);
            completed++;
        }

        return completed;
    }

    public async Task<int> ProcessDueAnonymizationsAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.UtcNow;
        List<OrganizationClosure> due = await dbContext.OrganizationClosures
            .AsNoTracking()
            .Where(x => x.Status == OrganizationClosureStatus.Completed &&
                        x.DataDisposition == OrganizationDataDisposition.AnonymizeAfterRetention &&
                        x.RetentionUntilUtc != null &&
                        x.RetentionUntilUtc <= now)
            .OrderBy(x => x.RetentionUntilUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (OrganizationClosure closure in due)
        {
            await anonymizationService.AnonymizeAsync(
                closure.OrganizationId,
                closure.RetentionUntilUtc!.Value,
                cancellationToken);
        }

        return due.Count;
    }
}
