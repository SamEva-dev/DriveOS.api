using System.Reflection;

namespace DriveOS.ArchitectureTests;

public sealed class MarketplaceCommunicationNotificationBoundaryTests
{
    [Fact]
    public void ProfessionalMarketplace_Application_ShouldNotReferenceCommunicationEngagement()
    {
        Assembly assembly=typeof(
            DriveOS.Modules.ProfessionalMarketplace.Application.Notifications.IMarketplaceNotificationGateway).Assembly;

        Assert.DoesNotContain(
            assembly.GetReferencedAssemblies(),
            x=>(x.Name??string.Empty).StartsWith("DriveOS.Modules.CommunicationEngagement",StringComparison.Ordinal));
    }

    [Fact]
    public void FundingBilling_Application_ShouldNotReferenceCommunicationEngagement()
    {
        Assembly assembly=typeof(
            DriveOS.Modules.FundingBilling.Application.SupplierPayments.ISupplierFinanceNotificationGateway).Assembly;

        Assert.DoesNotContain(
            assembly.GetReferencedAssemblies(),
            x=>(x.Name??string.Empty).StartsWith("DriveOS.Modules.CommunicationEngagement",StringComparison.Ordinal));
    }
}
