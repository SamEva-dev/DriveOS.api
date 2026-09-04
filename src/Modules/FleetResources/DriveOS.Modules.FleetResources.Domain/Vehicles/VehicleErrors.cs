using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FleetResources.Domain.Vehicles;

public static class VehicleErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation("Fleet.Vehicle.InvalidIdentifier", "errors.fleet.vehicle.invalidIdentifier");
    public static readonly Error InvalidOrganization = Error.Validation("Fleet.Vehicle.InvalidOrganization", "errors.fleet.vehicle.invalidOrganization");
    public static readonly Error RegistrationRequired = Error.Validation("Fleet.Vehicle.RegistrationRequired", "errors.fleet.vehicle.registrationRequired");
    public static readonly Error TechnicalProfileRequired = Error.Validation("Fleet.Vehicle.TechnicalProfileRequired", "errors.fleet.vehicle.technicalProfileRequired");
    public static readonly Error InvalidCompliancePeriod = Error.Validation("Fleet.Vehicle.InvalidCompliancePeriod", "errors.fleet.vehicle.invalidCompliancePeriod");
    public static readonly Error InvalidOdometer = Error.Validation("Fleet.Vehicle.InvalidOdometer", "errors.fleet.vehicle.invalidOdometer");
    public static readonly Error InvalidOdometerDate = Error.Validation("Fleet.Vehicle.InvalidOdometerDate", "errors.fleet.vehicle.invalidOdometerDate");
    public static readonly Error RegistrationConflict = Error.Conflict("Fleet.Vehicle.RegistrationConflict", "errors.fleet.vehicle.registrationConflict");
    public static readonly Error NotFound = Error.NotFound("Fleet.Vehicle.NotFound", "errors.fleet.vehicle.notFound");
}
