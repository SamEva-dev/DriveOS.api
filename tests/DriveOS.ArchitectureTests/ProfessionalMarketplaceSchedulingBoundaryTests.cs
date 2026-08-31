using System.Reflection;

namespace DriveOS.ArchitectureTests;

public sealed class ProfessionalMarketplaceSchedulingBoundaryTests
{
    [Fact]
    public void ProfessionalMarketplace_Application_ShouldNotReferenceSchedulingCapacity()
    {
        Assembly assembly=typeof(
            DriveOS.Modules.ProfessionalMarketplace.Application.Engagements.IProfessionalSchedulingPreparationGateway).Assembly;

        string[] references=assembly.GetReferencedAssemblies()
            .Select(x=>x.Name??string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            references,
            x=>x.StartsWith("DriveOS.Modules.SchedulingCapacity",StringComparison.Ordinal));
    }
}
