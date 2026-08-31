using DriveOS.Modules.FundingBilling.Application.SupplierPayments;

namespace DriveOS.Api.BackgroundJobs;

internal sealed class SupplierSettlementOverdueWorker(
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    ILogger<SupplierSettlementOverdueWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int intervalHours=Math.Clamp(
            configuration.GetValue<int?>("FundingBilling:SupplierSettlements:OverdueCheckIntervalHours")??6,
            1,24);

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope=scopes.CreateScope();
                var automation=scope.ServiceProvider.GetRequiredService<ISupplierSettlementOverdueAutomation>();
                int updated=await automation.RunAsync(stoppingToken);
                if(updated>0)
                    logger.LogInformation("Supplier settlement overdue scan updated {Count} invoice(s).",updated);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"Supplier settlement overdue scan failed.");
            }

            await Task.Delay(TimeSpan.FromHours(intervalHours),stoppingToken);
        }
    }
}
