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

    [Fact]
    public void Vehicle_should_keep_odometer_monotonic_and_accept_idempotent_replay()
    {
        var org = new OrganizationId(Guid.NewGuid());
        var user = new UserId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var vehicle = Vehicle.Create(VehicleId.New(), org, org, null, "CC-123-CC", null, "Citroen", "C3",
            "Manual", "Petrol", true, ["B"]).Value;

        vehicle.RecordOdometer(42_000, now.AddMinutes(-2), now, user).IsSuccess.Should().BeTrue();
        vehicle.RecordOdometer(42_000, now.AddMinutes(-1), now, user).IsSuccess.Should().BeTrue();
        vehicle.CurrentOdometerKilometers.Should().Be(42_000);

        vehicle.RecordOdometer(41_999, now, now, user).Error.Should().Be(VehicleErrors.InvalidOdometer);
        vehicle.CurrentOdometerKilometers.Should().Be(42_000);
    }

    [Fact]
    public void Vehicle_should_reject_backdated_or_future_odometer_reading()
    {
        var org = new OrganizationId(Guid.NewGuid());
        var user = new UserId(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var vehicle = Vehicle.Create(VehicleId.New(), org, org, null, "DD-123-DD", null, "Renault", "Zoé",
            "Automatic", "Electric", true, ["B"]).Value;

        vehicle.RecordOdometer(12_000, now, now, user).IsSuccess.Should().BeTrue();
        vehicle.RecordOdometer(12_100, now.AddMinutes(-1), now, user).Error.Should().Be(VehicleErrors.InvalidOdometerDate);
        vehicle.RecordOdometer(12_100, now.AddMinutes(6), now, user).Error.Should().Be(VehicleErrors.InvalidOdometerDate);
    }
}
