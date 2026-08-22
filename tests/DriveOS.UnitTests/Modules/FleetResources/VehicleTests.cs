using DriveOS.Modules.FleetResources.Domain.Vehicles;
using DriveOS.SharedKernel.Identifiers;
using FluentAssertions;

namespace DriveOS.UnitTests.FleetResources;

public sealed class VehicleTests
{
    [Fact]
    public void Vehicle_should_be_operational_only_with_complete_compliance()
    {
        var org = new OrganizationId(Guid.NewGuid());
        var user = new UserId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var created = Vehicle.Create(VehicleId.New(), org, org, null, "AA-123-AA", null, "Peugeot", "208",
            "Manual", "Petrol", true, ["B"]);
        created.IsSuccess.Should().BeTrue();
        created.Value.UpdateCompliance(true, true, now.AddMonths(6), false, now.AddMonths(3), VehicleOperationalStatus.Available, null, null, null, now, user);
        created.Value.IsOperationalFor(now.AddDays(1), now.AddDays(1).AddHours(1)).Should().BeTrue();
    }

    [Fact]
    public void Vehicle_should_block_when_insurance_expires_before_requested_period()
    {
        var org = new OrganizationId(Guid.NewGuid());
        var user = new UserId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var vehicle = Vehicle.Create(VehicleId.New(), org, org, null, "BB-123-BB", null, "Renault", "Clio",
            "Manual", "Petrol", true, ["B"]).Value;
        vehicle.UpdateCompliance(true, true, now.AddHours(1), false, now.AddMonths(3), VehicleOperationalStatus.Available, null, null, null, now, user);
        vehicle.IsOperationalFor(now.AddHours(2), now.AddHours(3)).Should().BeFalse();
    }
}
