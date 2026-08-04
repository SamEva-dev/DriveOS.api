using DomainRelay.Mapping.Abstractions.Configuration;
using DomainRelay.Mapping.DependencyInjection.Extensions;
using DriveOS.Api.Mapping;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.UnitTests.Mapping;

public sealed class OrganizationSettingsApiMappingProfileTests
{
    [Fact]
    public void MappingConfiguration_ShouldBeValid()
    {
        var services = new ServiceCollection();

        services.AddDomainRelayMapping(builder =>
        {
            builder
                .AddProfile<OrganizationSettingsApiMappingProfile>()
                .ValidateConfigurationOnBuild();
        });

        using ServiceProvider provider = services.BuildServiceProvider();

        IMapperConfigurationProvider configurationProvider =
            provider.GetRequiredService<IMapperConfigurationProvider>();

        configurationProvider.AssertConfigurationIsValid();
    }
}
