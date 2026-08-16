using DriveOS.Modules.Students.Application.Reactivations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Students.Infrastructure.Reactivations;

internal sealed class EnrollmentReactivationScheduler(
    IServiceScopeFactory scopes,
    ILogger<EnrollmentReactivationScheduler> logger
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
                    .ServiceProvider.GetRequiredService<IEnrollmentReactivationService>()
                    .ApplyDueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to apply scheduled enrollment reactivations.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
