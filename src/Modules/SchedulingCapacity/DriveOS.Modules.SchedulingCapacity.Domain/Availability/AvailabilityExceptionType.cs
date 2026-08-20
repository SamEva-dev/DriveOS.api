namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public enum AvailabilityExceptionType
{
    Available = 1,
    Unavailable = 2,
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
