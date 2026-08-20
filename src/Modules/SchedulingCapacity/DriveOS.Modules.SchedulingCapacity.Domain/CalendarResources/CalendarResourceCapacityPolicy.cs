namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

public static class CalendarResourceCapacityPolicy
{
    public static bool SupportsMultiplePlaces(CalendarResourceType resourceType) => resourceType is
        CalendarResourceType.Room or
        CalendarResourceType.Branch or
        CalendarResourceType.Simulator or
        CalendarResourceType.Equipment or
        CalendarResourceType.PartnerResource or
        CalendarResourceType.Other;

    public static bool IsValid(CalendarResourceType resourceType, int capacity) =>
        capacity is >= 1 and <= 10000 &&
        (SupportsMultiplePlaces(resourceType) || capacity == 1);
}
