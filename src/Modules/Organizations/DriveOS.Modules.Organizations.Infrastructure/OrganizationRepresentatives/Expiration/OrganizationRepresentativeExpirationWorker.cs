using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Application.OrganizationRepresentatives.Expiration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DriveOS.Modules.Organizations.Infrastructure.OrganizationRepresentatives.Expiration;

internal sealed class OrganizationRepresentativeExpirationWorker(
    IServiceScopeFactory scopeFactory,
    IClock clock,
    IOptions<OrganizationRepresentativeExpirationOptions> options,
    ILogger<OrganizationRepresentativeExpirationWorker> logger)
    : BackgroundService
{
    private readonly OrganizationRepresentativeExpirationOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        using PeriodicTimer timer = new(TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes)));
        do
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOrganizationRepresentativeExpirationProcessor>();
                int processed = await processor.ProcessAsync(
                    DateOnly.FromDateTime(clock.UtcNow.UtcDateTime),
                    Math.Clamp(_options.BatchSize, 1, 1000),
                    stoppingToken);

                if (processed > 0)
                    logger.LogInformation("Expired {Count} organization representative relations.", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Organization representative expiration processing failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
