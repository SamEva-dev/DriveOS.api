using System.Reflection;
using DriveOS.Api.Endpoints.OrganizationSettings;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;

namespace DriveOS.ArchitectureTests;

public sealed class OrganizationSettingsEndpointArchitectureTests
{
    [Fact]
    public void Endpoints_ShouldNotExposeAggregateOrRepositoryInMethodSignatures()
    {
        Type endpointType = typeof(OrganizationSettingsEndpoints);
        Type aggregateType = typeof(DriveOS.Modules.Organizations.Domain.OrganizationSettings.OrganizationSettings);
        Type repositoryType = typeof(IOrganizationSettingsRepository);

        MethodInfo[] methods = endpointType.GetMethods(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Static);

        foreach (MethodInfo method in methods)
        {
            Assert.False(
                ContainsForbiddenType(method.ReturnType, aggregateType, repositoryType),
                $"{endpointType.FullName}.{method.Name} exposes a forbidden return type.");

            foreach (ParameterInfo parameter in method.GetParameters())
            {
                Assert.False(
                    ContainsForbiddenType(parameter.ParameterType, aggregateType, repositoryType),
                    $"{endpointType.FullName}.{method.Name} exposes forbidden parameter '{parameter.Name}'.");
            }
        }
    }

    private static bool ContainsForbiddenType(
        Type candidate,
        Type aggregateType,
        Type repositoryType)
    {
        if (candidate == aggregateType || candidate == repositoryType)
        {
            return true;
        }

        if (candidate.IsGenericType)
        {
            return candidate.GetGenericArguments()
                .Any(type => ContainsForbiddenType(type, aggregateType, repositoryType));
        }

        return false;
    }
}
