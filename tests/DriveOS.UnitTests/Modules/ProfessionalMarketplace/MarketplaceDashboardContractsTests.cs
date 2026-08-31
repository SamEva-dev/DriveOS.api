using DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class MarketplaceDashboardContractsTests
{
    [Fact]
    public void Dashboard_kpis_keep_payment_and_validation_delays_separate()
    {
        var kpis=new MarketplaceDashboardKpis(
            2,3,4,1,2,2,1,1,5,0,2,12.5m,48.25m);

        Assert.Equal(12.5m,kpis.AverageValidationDelayHours);
        Assert.Equal(48.25m,kpis.AveragePaymentDelayHours);
        Assert.NotEqual(kpis.AverageValidationDelayHours,kpis.AveragePaymentDelayHours);
    }

    [Fact]
    public void Alerts_use_stable_message_keys()
    {
        var alert=new MarketplaceDashboardAlert(
            "payments.failed",
            "critical",
            "professionalMarketplace.dashboard.alerts.failedPayments",
            null,
            "ProfessionalInvoice",
            null);

        Assert.StartsWith("professionalMarketplace.dashboard.alerts.",alert.MessageKey);
    }
}
