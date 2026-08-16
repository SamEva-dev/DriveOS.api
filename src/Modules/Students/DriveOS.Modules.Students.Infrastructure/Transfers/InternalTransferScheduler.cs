using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Students.Domain.Transfers;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Students.Infrastructure.Transfers;

internal sealed class InternalTransferScheduler(
    IServiceScopeFactory scopes,
    IClock clock,
    ILogger<InternalTransferScheduler> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to apply scheduled internal student transfers.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StudentsDbContext>();
        var today = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        var transfers = await db
            .InternalTransferCases.Where(x =>
                (x.Status == InternalTransferStatus.Scheduled && x.EffectiveOn <= today)
                || (
                    x.Status == InternalTransferStatus.Applied
                    && x.Mode == InternalTransferMode.Temporary
                    && x.TemporaryUntil.HasValue
                    && x.TemporaryUntil.Value < today
                )
            )
            .ToListAsync(ct);
        foreach (var transfer in transfers)
        {
            var enrollment = await db
                .Enrollments.Where(x =>
                    x.OrganizationId == transfer.OrganizationId && x.StudentId == transfer.StudentId
                )
                .OrderByDescending(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync(ct);
            if (enrollment is null)
            {
                logger.LogWarning(
                    "Enrollment missing for internal transfer {TransferId}.",
                    transfer.Id
                );
                continue;
            }
            if (transfer.Status == InternalTransferStatus.Scheduled)
            {
                enrollment.TransferToBranch(
                    transfer.TargetBranchId,
                    transfer.ValidatedByUserId ?? transfer.AnalyzedByUserId,
                    clock.UtcNow
                );
                transfer.ApplyScheduled(today);
            }
            else
            {
                enrollment.TransferToBranch(
                    transfer.SourceBranchId,
                    transfer.ValidatedByUserId ?? transfer.AnalyzedByUserId,
                    clock.UtcNow
                );
                transfer.RevertTemporary(today);
            }
        }
        if (transfers.Count > 0)
            await db.SaveChangesAsync(ct);
    }
}
