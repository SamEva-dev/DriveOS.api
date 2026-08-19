using FluentAssertions;
using DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DriveOS.UnitTests.Modules.CurriculumPedagogy.Persistence;

public sealed class CurriculumPedagogyModelMappingTests
{
    [Fact]
    public void Model_Should_Build_With_All_StronglyTyped_Identifiers_Mapped()
    {
        var options = new DbContextOptionsBuilder<CurriculumPedagogyDbContext>()
            .UseNpgsql("Host=localhost;Database=driveos_model_validation;Username=test;Password=test")
            .Options;

        using var context = new CurriculumPedagogyDbContext(options);

        var model = context.Model;

        model.Should().NotBeNull();
    }

    [Fact]
    public void Nullable_UserId_Properties_Should_Have_ValueConverters()
    {
        var options = new DbContextOptionsBuilder<CurriculumPedagogyDbContext>()
            .UseNpgsql("Host=localhost;Database=driveos_model_validation;Username=test;Password=test")
            .Options;

        using var context = new CurriculumPedagogyDbContext(options);

        var unmappedProperties = context.Model
            .GetEntityTypes()
            .SelectMany(entityType => entityType.GetProperties())
            .Where(property => property.ClrType == typeof(UserId?))
            .Where(property => property.GetValueConverter() is null)
            .Select(property => $"{property.DeclaringType.Name}.{property.Name}")
            .ToArray();

        unmappedProperties.Should().BeEmpty();
    }
}
