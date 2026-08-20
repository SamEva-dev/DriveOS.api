namespace DriveOS.Modules.SchedulingCapacity.Domain.Availability;

public static class AvailabilityExceptionPolicy
{
    public static bool IsAvailable(AvailabilityExceptionType type) => type == AvailabilityExceptionType.Available;

    public static bool IsUnavailable(AvailabilityExceptionType type) => !IsAvailable(type);

    public static AvailabilityExceptionSource ResolveSource(AvailabilityExceptionType type) => type switch
    {
        AvailabilityExceptionType.Available => AvailabilityExceptionSource.SelfDeclared,
        AvailabilityExceptionType.Unavailable => AvailabilityExceptionSource.SelfDeclared,
        AvailabilityExceptionType.Maintenance => AvailabilityExceptionSource.Maintenance,
        AvailabilityExceptionType.Reservation => AvailabilityExceptionSource.Reservation,
        AvailabilityExceptionType.Breakdown => AvailabilityExceptionSource.Breakdown,
        AvailabilityExceptionType.Cleaning => AvailabilityExceptionSource.Cleaning,
        AvailabilityExceptionType.Inspection => AvailabilityExceptionSource.Inspection,
        AvailabilityExceptionType.Transfer => AvailabilityExceptionSource.Transfer,
        AvailabilityExceptionType.Closure => AvailabilityExceptionSource.Closure,
        AvailabilityExceptionType.Rental => AvailabilityExceptionSource.Rental,
        AvailabilityExceptionType.RecurringRule => AvailabilityExceptionSource.RecurringRule,
        AvailabilityExceptionType.PartnerRestriction => AvailabilityExceptionSource.PartnerRestriction,
        AvailabilityExceptionType.Other => AvailabilityExceptionSource.Other,
        _ => AvailabilityExceptionSource.Other
    };

    public static int DefaultPriority(AvailabilityExceptionType type, AvailabilityExceptionSource source) =>
        type switch
        {
            AvailabilityExceptionType.Breakdown => 1000,
            AvailabilityExceptionType.Maintenance => 950,
            AvailabilityExceptionType.Inspection => 940,
            AvailabilityExceptionType.Unavailable when source == AvailabilityExceptionSource.Absence => 900,
            AvailabilityExceptionType.Unavailable => 850,
            AvailabilityExceptionType.PartnerRestriction => 825,
            AvailabilityExceptionType.Transfer => 800,
            AvailabilityExceptionType.Closure => 800,
            AvailabilityExceptionType.Rental => 780,
            AvailabilityExceptionType.Reservation => 750,
            AvailabilityExceptionType.Cleaning => 700,
            AvailabilityExceptionType.Available => 500,
            _ => 600
        };
}
