namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public enum AvailabilityExceptionSource
{
    Unknown = 0,
    Contract = 1,
    SelfDeclared = 2,
    HrSchedule = 3,
    Mission = 4,
    Absence = 5,
    OrganizationRule = 6,
    StudentDeclared = 7,
    Maintenance = 10,
    Reservation = 11,
    Breakdown = 12,
    Cleaning = 13,
    Inspection = 14,
    Transfer = 15,
    Closure = 16,
    Rental = 17,
    RecurringRule = 18,
    PartnerRestriction = 19,
    Other = 99
}
