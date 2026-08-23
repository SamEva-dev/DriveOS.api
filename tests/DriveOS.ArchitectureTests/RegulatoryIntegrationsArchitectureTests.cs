using System.Reflection;
using DriveOS.Modules.RegulatoryIntegrations.Domain.Submissions;

namespace DriveOS.ArchitectureTests;

public sealed class RegulatoryIntegrationsArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotReferenceOuterLayers() =>
        AssertDoesNotReference(
            typeof(RegulatoryTrainingRecordSubmission).Assembly,
            [
                "DriveOS.Modules.RegulatoryIntegrations.Application",
                "DriveOS.Modules.RegulatoryIntegrations.Infrastructure",
                "DriveOS.Api",
            ]
        );

    [Fact]
    public void Application_ShouldNotReferenceInfrastructureOrApi() =>
        AssertDoesNotReference(
            typeof(DriveOS.Modules.RegulatoryIntegrations.Application.Submissions.IRegulatoryTrainingRecordSubmissionService).Assembly,
            [
                "DriveOS.Modules.RegulatoryIntegrations.Infrastructure",
                "DriveOS.Api",
            ]
        );

    [Fact]
    public void SubmissionAggregate_ShouldUseStrongIdentifier()
    {
        Type? aggregateRoot = GetAggregateRootBase(typeof(RegulatoryTrainingRecordSubmission));

        Assert.NotNull(aggregateRoot);
        Assert.NotEqual(typeof(Guid), aggregateRoot!.GetGenericArguments()[0]);
    }

    [Fact]
    public void ObsoleteLivretNumeriquePlaceholders_ShouldNotRemainInApiAssembly()
    {
        string[] obsoleteTypes =
        [
            "FrenchLivretNumeriqueProviderPlaceholder",
            "FrenchLivretNumeriqueTransportProviderPlaceholder",
        ];

        string[] apiTypes = typeof(global::Program).Assembly
            .GetTypes()
            .Select(type => type.Name)
            .ToArray();

        foreach (string obsoleteType in obsoleteTypes)
            Assert.DoesNotContain(obsoleteType, apiTypes);
    }

    private static Type? GetAggregateRootBase(Type type)
    {
        for (Type? current = type.BaseType; current is not null; current = current.BaseType)
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(DriveOS.SharedKernel.Domain.AggregateRoot<>))
                return current;

        return null;
    }

    private static void AssertDoesNotReference(
        Assembly assembly,
        IReadOnlyCollection<string> forbiddenPrefixes)
    {
        string[] references = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        foreach (string prefix in forbiddenPrefixes)
            Assert.DoesNotContain(
                references,
                reference => reference.StartsWith(prefix, StringComparison.Ordinal));
    }
}
