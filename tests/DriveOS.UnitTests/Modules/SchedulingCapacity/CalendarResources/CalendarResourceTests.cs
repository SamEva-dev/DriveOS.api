using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentAssertions;

namespace DriveOS.UnitTests.Modules.SchedulingCapacity.CalendarResources;

public sealed class CalendarResourceTests
{
    [Fact]
    public void Create_ShouldCreateActiveResource_WhenDataIsValid()
    {
        Result<CalendarResource> result = CalendarResource.Create(
            CalendarResourceId.New(),
            new OrganizationId(Guid.NewGuid()),
            new BranchId(Guid.NewGuid()),
            CalendarResourceType.Instructor,
            Guid.NewGuid(),
            "Moniteur principal",
            1,
            "Europe/Paris");

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(CalendarResourceStatus.Active);
        result.Value.Capacity.Should().Be(1);
    }

    [Fact]
    public void Create_ShouldFail_WhenExternalReferenceIsEmpty()
    {
        Result<CalendarResource> result = CalendarResource.Create(
            CalendarResourceId.New(),
            new OrganizationId(Guid.NewGuid()),
            null,
            CalendarResourceType.Vehicle,
            Guid.Empty,
            "Véhicule B",
            1,
            "Europe/Paris");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CalendarResourceErrors.InvalidExternalResource);
    }

    [Fact]
    public void Restrict_ShouldPreserveResourceAndPreventNormalAvailability()
    {
        CalendarResource resource = CreateResource();

        var result = resource.Restrict("Qualification expirante");

        result.IsSuccess.Should().BeTrue();
        resource.Status.Should().Be(CalendarResourceStatus.Restricted);
        resource.RestrictionReason.Should().Be("Qualification expirante");
    }

    [Fact]
    public void MarkUnavailable_ThenActivate_ShouldRestoreActiveResource()
    {
        CalendarResource resource = CreateResource();

        resource.MarkUnavailable("Maintenance").IsSuccess.Should().BeTrue();
        resource.Activate().IsSuccess.Should().BeTrue();

        resource.Status.Should().Be(CalendarResourceStatus.Active);
        resource.UnavailabilityReason.Should().BeNull();
    }

    [Fact]
    public void ArchivedResource_CannotBeModifiedOrReactivated()
    {
        CalendarResource resource = CreateResource();
        resource.Archive().IsSuccess.Should().BeTrue();

        resource.UpdateMetadata(null, "Nouveau nom", 2, "Europe/Paris").IsFailure.Should().BeTrue();
        resource.Activate().IsFailure.Should().BeTrue();
        resource.Status.Should().Be(CalendarResourceStatus.Archived);
    }

    [Fact]
    public void Create_ShouldRejectMultiPlaceCapacity_ForExclusiveResource()
    {
        Result<CalendarResource> result = CalendarResource.Create(
            CalendarResourceId.New(),
            OrganizationId.New(),
            BranchId.New(),
            CalendarResourceType.Instructor,
            Guid.NewGuid(),
            "Moniteur",
            2,
            "Europe/Paris");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CalendarResourceErrors.InvalidCapacity);
    }

    [Theory]
    [InlineData(CalendarResourceType.Room, 24)]
    [InlineData(CalendarResourceType.Branch, 40)]
    [InlineData(CalendarResourceType.Simulator, 2)]
    [InlineData(CalendarResourceType.Equipment, 8)]
    [InlineData(CalendarResourceType.PartnerResource, 16)]
    public void Create_ShouldAllowMultiPlaceCapacity_ForCollectiveResources(CalendarResourceType resourceType, int capacity)
    {
        Result<CalendarResource> result = CalendarResource.Create(
            CalendarResourceId.New(),
            OrganizationId.New(),
            BranchId.New(),
            resourceType,
            Guid.NewGuid(),
            "Ressource collective",
            capacity,
            "Europe/Paris");

        result.IsSuccess.Should().BeTrue();
        result.Value.Capacity.Should().Be(capacity);
    }


    [Fact]
    public void Create_ShouldKeepExamVehicleExclusive()
    {
        Result<CalendarResource> result = CalendarResource.Create(
            CalendarResourceId.New(),
            OrganizationId.New(),
            BranchId.New(),
            CalendarResourceType.ExamVehicle,
            Guid.NewGuid(),
            "Véhicule examen",
            2,
            "Europe/Paris");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CalendarResourceErrors.InvalidCapacity);
    }

    private static CalendarResource CreateResource()
    {
        return CalendarResource.Create(
            CalendarResourceId.New(),
            new OrganizationId(Guid.NewGuid()),
            null,
            CalendarResourceType.Room,
            Guid.NewGuid(),
            "Salle 1",
            12,
            "Europe/Paris").Value;
    }
}
