using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationClosures;

public interface IOrganizationClosureScheduler
{
    Task<int> ProcessDueClosuresAsync(CancellationToken cancellationToken);
    Task<int> ProcessDueAnonymizationsAsync(CancellationToken cancellationToken);
}

public sealed class OrganizationClosureWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OrganizationClosureWorker> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));
        while (
            !stoppingToken.IsCancellationRequested
            && await timer.WaitForNextTickAsync(stoppingToken)
        )
        {
            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                var scheduler =
                    scope.ServiceProvider.GetRequiredService<IOrganizationClosureScheduler>();
                await scheduler.ProcessDueClosuresAsync(stoppingToken);
                await scheduler.ProcessDueAnonymizationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Organization closure worker iteration failed.");
            }
        }
    }
}
