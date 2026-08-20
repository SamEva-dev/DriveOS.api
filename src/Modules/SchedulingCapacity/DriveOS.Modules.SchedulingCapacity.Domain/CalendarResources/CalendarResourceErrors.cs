using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;

public static class CalendarResourceErrors
{
    public static readonly Error InvalidIdentifier = Error.Validation(
        "SchedulingCapacity.CalendarResource.Id.Invalid",
        "errors.schedulingCapacity.calendarResource.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "SchedulingCapacity.CalendarResource.Organization.Invalid",
        "errors.schedulingCapacity.calendarResource.organization.invalid");

    public static readonly Error InvalidBranch = Error.Validation(
        "SchedulingCapacity.CalendarResource.Branch.Invalid",
        "errors.schedulingCapacity.calendarResource.branch.invalid");

    public static readonly Error InvalidExternalResource = Error.Validation(
        "SchedulingCapacity.CalendarResource.ExternalResource.Invalid",
        "errors.schedulingCapacity.calendarResource.externalResource.invalid");

    public static readonly Error InvalidType = Error.Validation(
        "SchedulingCapacity.CalendarResource.Type.Invalid",
        "errors.schedulingCapacity.calendarResource.type.invalid");

    public static readonly Error InvalidDisplayName = Error.Validation(
        "SchedulingCapacity.CalendarResource.DisplayName.Invalid",
        "errors.schedulingCapacity.calendarResource.displayName.invalid");

    public static readonly Error InvalidCapacity = Error.Validation(
        "SchedulingCapacity.CalendarResource.Capacity.Invalid",
        "errors.schedulingCapacity.calendarResource.capacity.invalid");

    public static readonly Error InvalidTimeZone = Error.Validation(
        "SchedulingCapacity.CalendarResource.TimeZone.Invalid",
        "errors.schedulingCapacity.calendarResource.timeZone.invalid");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "SchedulingCapacity.CalendarResource.Modification.NotAllowed",
        "errors.schedulingCapacity.calendarResource.modification.notAllowed");

    public static readonly Error RestrictionNotAllowed = Error.Conflict(
        "SchedulingCapacity.CalendarResource.Restriction.NotAllowed",
        "errors.schedulingCapacity.calendarResource.restriction.notAllowed");

    public static readonly Error AvailabilityChangeNotAllowed = Error.Conflict(
        "SchedulingCapacity.CalendarResource.AvailabilityChange.NotAllowed",
        "errors.schedulingCapacity.calendarResource.availabilityChange.notAllowed");

    public static readonly Error ActivationNotAllowed = Error.Conflict(
        "SchedulingCapacity.CalendarResource.Activation.NotAllowed",
        "errors.schedulingCapacity.calendarResource.activation.notAllowed");

    public static readonly Error ArchiveNotAllowed = Error.Conflict(
        "SchedulingCapacity.CalendarResource.Archive.NotAllowed",
        "errors.schedulingCapacity.calendarResource.archive.notAllowed");
}
