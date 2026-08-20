using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public interface IBookingReferenceValidationGateway
{
    Task<Error?> ValidateAsync(
        OrganizationId organizationId,
        BranchId? branchId,
        int bookingType,
        string? trainingCategory,
        IReadOnlyCollection<CreateBookingResourceRequest> resources,
        IReadOnlyCollection<CreateBookingParticipantRequest> participants,
        CancellationToken cancellationToken = default);
}

public static class BookingReferenceValidationErrors
{
    public static readonly Error CalendarResourceNotFound = Error.NotFound(
        "SchedulingCapacity.Booking.CalendarResourceNotFound",
        "errors.schedulingCapacity.booking.calendarResourceNotFound");

    public static readonly Error StudentNotFound = Error.NotFound(
        "SchedulingCapacity.Booking.StudentNotFound",
        "errors.schedulingCapacity.booking.studentNotFound");

    public static readonly Error ParticipantCalendarResourceRequired = Error.Validation(
        "SchedulingCapacity.Booking.ParticipantCalendarResourceRequired",
        "errors.schedulingCapacity.booking.participantCalendarResourceRequired");

    public static readonly Error ResourceBranchMismatch = Error.Validation(
        "SchedulingCapacity.Booking.ResourceBranchMismatch",
        "errors.schedulingCapacity.booking.resourceBranchMismatch");

    public static readonly Error ResourceQuantityExceedsCapacity = Error.Validation(
        "SchedulingCapacity.Booking.ResourceQuantityExceedsCapacity",
        "errors.schedulingCapacity.booking.resourceQuantityExceedsCapacity");

    public static readonly Error StudentParticipantRequired = Error.Validation(
        "SchedulingCapacity.Booking.StudentParticipantRequired",
        "errors.schedulingCapacity.booking.studentParticipantRequired");

    public static readonly Error InstructorParticipantRequired = Error.Validation(
        "SchedulingCapacity.Booking.InstructorParticipantRequired",
        "errors.schedulingCapacity.booking.instructorParticipantRequired");

    public static readonly Error TrainingCategoryRequired = Error.Validation(
        "SchedulingCapacity.Booking.TrainingCategoryRequired",
        "errors.schedulingCapacity.booking.trainingCategoryRequired");

    public static readonly Error InstructorNotEligible = Error.Validation(
        "SchedulingCapacity.Booking.InstructorNotEligible",
        "errors.schedulingCapacity.booking.instructorNotEligible");
}
