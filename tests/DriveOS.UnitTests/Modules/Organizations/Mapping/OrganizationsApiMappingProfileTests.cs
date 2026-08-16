using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DriveOS.Api.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.UnitTests.Modules.Organizations.Mapping;

public sealed class OrganizationsApiMappingProfileTests
{
    [Fact]
    public void MappingConfiguration_ShouldBeValid()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder.AddProfile<OrganizationsApiMappingProfile>().ValidateConfigurationOnBuild();
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        IMapperConfigurationProvider configurationProvider =
            provider.GetRequiredService<IMapperConfigurationProvider>();

        configurationProvider.AssertConfigurationIsValid();
    }
}
