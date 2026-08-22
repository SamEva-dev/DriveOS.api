namespace DriveOS.Modules.FleetResources.Domain.Vehicles;

public enum VehicleOperationalStatus
{
    Expected = 0,
    Available = 1,
    Reserved = 2,
    Assigned = 3,
    InUse = 4,
    MaintenanceDue = 5,
    UnderMaintenance = 6,
    Restricted = 7,
    Immobilized = 8,
    OutOfService = 9,
    Returning = 10,
    Returned = 11
}
