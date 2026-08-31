using DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence;
using DriveOS.Modules.Contracts.Infrastructure.Persistence;
using DriveOS.Modules.FundingBilling.Infrastructure.Persistence;
using DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DriveOS.UnitTests.Modules.Persistence;

public sealed class StronglyTypedIdentifierModelMappingTests
{
    private const string ConnectionString =
        "Host=localhost;Database=driveos_model_validation;Username=test;Password=test";

    [Fact]
    public void CommunicationEngagement_model_should_map_all_user_identifiers()
    {
        var options = new DbContextOptionsBuilder<CommunicationEngagementDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new CommunicationEngagementDbContext(options);

        AssertUserIdentifiersAreMapped(context);
    }

    [Fact]
    public void ProfessionalMarketplace_model_should_map_all_user_identifiers()
    {
        var options = new DbContextOptionsBuilder<ProfessionalMarketplaceDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new ProfessionalMarketplaceDbContext(options);

        AssertUserIdentifiersAreMapped(context);
    }

    [Fact]
    public void Contracts_model_should_map_all_user_identifiers()
    {
        var options = new DbContextOptionsBuilder<ContractsDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new ContractsDbContext(options);

        AssertUserIdentifiersAreMapped(context);
    }

    [Fact]
    public void FundingBilling_model_should_map_all_user_identifiers()
    {
        var options = new DbContextOptionsBuilder<FundingBillingDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new FundingBillingDbContext(options);

        AssertUserIdentifiersAreMapped(context);
    }

    private static void AssertUserIdentifiersAreMapped(DbContext context)
    {
        var unmappedProperties = context.Model
            .GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType == typeof(UserId) || property.ClrType == typeof(UserId?))
            .Where(property => property.GetValueConverter() is null)
            .Select(property => $"{property.DeclaringType.Name}.{property.Name}")
            .ToArray();

        unmappedProperties.Should().BeEmpty();
    }
}
