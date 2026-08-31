using System.Reflection;

namespace DriveOS.ArchitectureTests;

public sealed class ProfessionalMarketplaceWorkforceBoundaryTests
{
    [Fact]
    public void ProfessionalMarketplace_Domain_ShouldNotReferenceWorkforce()
    {
        AssertDoesNotReference(
            typeof(DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles.ProfessionalProfile).Assembly,
            "DriveOS.Modules.Workforce");
    }

    [Fact]
    public void ProfessionalMarketplace_Application_ShouldNotReferenceWorkforce()
    {
        AssertDoesNotReference(
            typeof(DriveOS.Modules.ProfessionalMarketplace.Application.Engagements.IProfessionalEngagementOperationalReadService).Assembly,
            "DriveOS.Modules.Workforce");
    }

    private static void AssertDoesNotReference(Assembly assembly,string forbiddenPrefix)
    {
        string[] references=assembly.GetReferencedAssemblies()
            .Select(x=>x.Name??string.Empty)
            .ToArray();

        Assert.DoesNotContain(
            references,
            x=>x.StartsWith(forbiddenPrefix,StringComparison.Ordinal));
    }
}
