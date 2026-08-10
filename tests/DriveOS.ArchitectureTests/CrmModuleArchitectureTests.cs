using System.Reflection;

namespace DriveOS.ArchitectureTests;

public sealed class CrmModuleArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotReferenceOuterLayers()
    {
        Assembly domainAssembly =
            typeof(DriveOS.Modules.CRM.Domain.AssemblyMarker).Assembly;

        string[] forbiddenPrefixes =
        [
            "DriveOS.Modules.CRM.Application",
            "DriveOS.Modules.CRM.Infrastructure",
            "DriveOS.Api"
        ];

        AssertDoesNotReference(domainAssembly, forbiddenPrefixes);
    }

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureOrApi()
    {
        Assembly applicationAssembly =
            typeof(DriveOS.Modules.CRM.Application.AssemblyMarker).Assembly;

        string[] forbiddenPrefixes =
        [
            "DriveOS.Modules.CRM.Infrastructure",
            "DriveOS.Api"
        ];

        AssertDoesNotReference(applicationAssembly, forbiddenPrefixes);
    }

    private static void AssertDoesNotReference(
        Assembly assembly,
        IReadOnlyCollection<string> forbiddenPrefixes)
    {
        string[] references = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        foreach (string forbiddenPrefix in forbiddenPrefixes)
        {
            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(
                    forbiddenPrefix,
                    StringComparison.Ordinal));
        }
    }
}
