using System.Reflection;

namespace DriveOS.ArchitectureTests;

public sealed class StudentsModuleArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotReferenceOuterLayers() =>
        AssertDoesNotReference(
            typeof(DriveOS.Modules.Students.Domain.AssemblyMarker).Assembly,
            [
                "DriveOS.Modules.Students.Application",
                "DriveOS.Modules.Students.Infrastructure",
                "DriveOS.Api",
            ]
        );

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureOrApi() =>
        AssertDoesNotReference(
            typeof(DriveOS.Modules.Students.Application.AssemblyMarker).Assembly,
            ["DriveOS.Modules.Students.Infrastructure", "DriveOS.Api"]
        );

    [Fact]
    public void AggregateRoots_ShouldUseStrongIdentifiers()
    {
        Type[] invalid = typeof(DriveOS.Modules.Students.Domain.AssemblyMarker).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract)
            .Where(type => GetAggregateRootBase(type) is not null)
            .Where(type => GetAggregateRootBase(type)!.GetGenericArguments()[0] == typeof(Guid))
            .ToArray();

        Assert.Empty(invalid);

        static Type? GetAggregateRootBase(Type type)
        {
            for (Type? current = type.BaseType; current is not null; current = current.BaseType)
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(DriveOS.SharedKernel.Domain.AggregateRoot<>))
                    return current;
            return null;
        }
    }

    private static void AssertDoesNotReference(
        Assembly assembly,
        IReadOnlyCollection<string> forbiddenPrefixes
    )
    {
        string[] references = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();
        foreach (string prefix in forbiddenPrefixes)
            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(prefix, StringComparison.Ordinal)
            );
    }
}
