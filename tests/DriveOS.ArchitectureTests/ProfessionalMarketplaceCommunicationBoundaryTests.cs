using System.Reflection;
namespace DriveOS.ArchitectureTests;
public sealed class ProfessionalMarketplaceCommunicationBoundaryTests
{
    [Fact]
    public void ProfessionalMarketplace_Application_ShouldNotReferenceCommunicationEngagement()
    {
        Assembly assembly=typeof(DriveOS.Modules.ProfessionalMarketplace.Application.Messaging.IMarketplaceCommunicationGateway).Assembly;
        Assert.DoesNotContain(assembly.GetReferencedAssemblies(),x=>(x.Name??string.Empty).StartsWith("DriveOS.Modules.CommunicationEngagement",StringComparison.Ordinal));
    }
}
