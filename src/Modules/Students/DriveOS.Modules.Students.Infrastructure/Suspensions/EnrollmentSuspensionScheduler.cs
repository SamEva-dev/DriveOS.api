using DriveOS.Modules.Students.Application.Suspensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Students.Infrastructure.Suspensions;

internal sealed class EnrollmentSuspensionScheduler(
    IServiceScopeFactory scopes,
    ILogger<EnrollmentSuspensionScheduler> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                await scope
                    .ServiceProvider.GetRequiredService<IEnrollmentSuspensionService>()
                    .ActivateDueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to activate scheduled enrollment suspensions.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
