using DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;
using Xunit;

namespace DriveOS.UnitTests.Modules.ProfessionalMarketplace;

public sealed class MarketplaceAdvancedDashboardContractsTests
{
    [Fact]
    public void Advanced_kpis_keep_funnel_operations_quality_and_finance_separate()
    {
        var kpis=new MarketplaceDashboardAdvancedKpis(
            InvitationsSent:10,
            InvitationsAccepted:8,
            InvitationsActivated:6,
            InvitationAcceptanceRatePercent:80m,
            InvitationToActivationRatePercent:60m,
            ApplicationsDecided:5,
            ApplicationsAccepted:4,
            ApplicationAcceptanceRatePercent:80m,
            CompleteProfiles:7,
            ProfilesInScope:8,
            ProfileCompletionRatePercent:87.5m,
            AverageDocumentValidationDelayHours:12m,
            ContractPreparedEngagements:5,
            PlannedHours:100m,
            RealizedHours:75m,
            CancelledMissions:2,
            OccupancyRatePercent:75m,
            StudentsHandled:24,
            ReviewedServiceEntries:20,
            ServiceEntriesValidatedWithoutCorrection:18,
            FirstPassValidationRatePercent:90m,
            OverdueInvoices:3,
            OpenDisputes:1,
            AverageHourlyCost:42.50m,
            CostCurrency:"EUR");

        Assert.Equal(60m,kpis.InvitationToActivationRatePercent);
        Assert.Equal(75m,kpis.OccupancyRatePercent);
        Assert.Equal(90m,kpis.FirstPassValidationRatePercent);
        Assert.Equal("EUR",kpis.CostCurrency);
    }

    [Fact]
    public void Existing_dashboard_response_remains_backward_compatible()
    {
        var legacy=new MarketplaceDashboardKpis(1,2,3,0,1,1,0,0,4,0,0,2m,3m);
        var response=new OrganizationMarketplaceDashboardResponse(legacy,[]);

        Assert.Null(response.Advanced);
        Assert.Equal(1,response.Kpis.ActiveEngagements);
    }
}
