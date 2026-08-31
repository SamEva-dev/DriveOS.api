using DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;

namespace DriveOS.Api.BackgroundJobs;

internal sealed class ProfessionalComplianceExpirationWorker(
    IServiceScopeFactory scopes,
    IConfiguration configuration,
    ILogger<ProfessionalComplianceExpirationWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int intervalHours=Math.Clamp(
            configuration.GetValue<int?>("ProfessionalMarketplace:ComplianceExpiration:IntervalHours")??6,
            1,24);

        int warningDays=Math.Clamp(
            configuration.GetValue<int?>("ProfessionalMarketplace:ComplianceExpiration:WarningDays")??30,
            1,180);

        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope=scopes.CreateScope();
                var automation=scope.ServiceProvider.GetRequiredService<IProfessionalComplianceExpirationAutomation>();
                ProfessionalComplianceExpirationRunResult result=
                    await automation.RunAsync(warningDays,stoppingToken);

                logger.LogInformation(
                    "Professional compliance expiration run: expiringDocs={ExpiringDocs} expiredDocs={ExpiredDocs} expiredCredentials={ExpiredCredentials} profiles={Profiles} notifications={Notifications}",
                    result.DocumentsMarkedExpiringSoon,result.DocumentsExpired,result.CredentialsExpired,
                    result.ProfilesReevaluated,result.NotificationsQueued);
            }
            catch(OperationCanceledException) when(stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch(Exception ex)
            {
                logger.LogError(ex,"Professional compliance expiration automation failed.");
            }

            await Task.Delay(TimeSpan.FromHours(intervalHours),stoppingToken);
        }
    }
}
