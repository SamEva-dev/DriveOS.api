namespace DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;

public enum SchedulingConflictType
{
    InstructorOverlap = 1,
    StudentOverlap = 2,
    VehicleOverlap = 3,
    RoomOverlap = 4,
    TravelTimeConflict = 5,
    QualificationConflict = 6,
    WorkingTimeViolation = 7,
    DocumentRestriction = 8,
    FinancialRestriction = 9,
    MaintenanceConflict = 10,
    LocationConflict = 11,
    CapacityConflict = 12,
    ResourceUnavailable = 13,
    AdministrativeBlock = 14,
    CreditInsufficient = 15,
    Other = 99
}
