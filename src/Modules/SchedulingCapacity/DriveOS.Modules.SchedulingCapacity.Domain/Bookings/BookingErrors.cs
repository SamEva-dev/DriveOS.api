using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public static class BookingErrors
{

    public static readonly Error InvalidCreationIdempotency = Error.Validation(
        "SchedulingCapacity.Booking.Creation.Idempotency.Invalid",
        "errors.schedulingCapacity.booking.creation.idempotency.invalid");

    public static readonly Error CreationIdempotencyConflict = Error.Conflict(
        "SchedulingCapacity.Booking.Creation.Idempotency.Conflict",
        "errors.schedulingCapacity.booking.creation.idempotency.conflict");

    public static readonly Error InvalidCreationDetails = Error.Validation(
        "SchedulingCapacity.Booking.Creation.Details.Invalid",
        "errors.schedulingCapacity.booking.creation.details.invalid");

    public static readonly Error CreditReservationRequired = Error.Conflict(
        "SchedulingCapacity.Booking.CreditReservation.Required",
        "errors.schedulingCapacity.booking.creditReservation.required");

    public static readonly Error CreditReservationFailed = Error.Conflict(
        "SchedulingCapacity.Booking.CreditReservation.Failed",
        "errors.schedulingCapacity.booking.creditReservation.failed");

    public static readonly Error CreditInsufficient = Error.Conflict(
        "SchedulingCapacity.Booking.Credit.Insufficient",
        "errors.schedulingCapacity.booking.credit.insufficient");

    public static readonly Error InvalidSlotHold = Error.Validation(
        "SchedulingCapacity.Booking.SlotHold.Invalid",
        "errors.schedulingCapacity.booking.slotHold.invalid");

    public static readonly Error InvalidIdentifier = Error.Validation(
        "SchedulingCapacity.Booking.Id.Invalid",
        "errors.schedulingCapacity.booking.id.invalid");

    public static readonly Error InvalidOrganization = Error.Validation(
        "SchedulingCapacity.Booking.Organization.Invalid",
        "errors.schedulingCapacity.booking.organization.invalid");

    public static readonly Error InvalidBranch = Error.Validation(
        "SchedulingCapacity.Booking.Branch.Invalid",
        "errors.schedulingCapacity.booking.branch.invalid");

    public static readonly Error InvalidType = Error.Validation(
        "SchedulingCapacity.Booking.Type.Invalid",
        "errors.schedulingCapacity.booking.type.invalid");

    public static readonly Error InvalidPeriod = Error.Validation(
        "SchedulingCapacity.Booking.Period.Invalid",
        "errors.schedulingCapacity.booking.period.invalid");

    public static readonly Error InvalidTitle = Error.Validation(
        "SchedulingCapacity.Booking.Title.Invalid",
        "errors.schedulingCapacity.booking.title.invalid");

    public static readonly Error ModificationNotAllowed = Error.Conflict(
        "SchedulingCapacity.Booking.Modification.NotAllowed",
        "errors.schedulingCapacity.booking.modification.notAllowed");

    public static readonly Error InvalidResource = Error.Validation(
        "SchedulingCapacity.Booking.Resource.Invalid",
        "errors.schedulingCapacity.booking.resource.invalid");

    public static readonly Error DuplicateResource = Error.Conflict(
        "SchedulingCapacity.Booking.Resource.Duplicate",
        "errors.schedulingCapacity.booking.resource.duplicate");

    public static readonly Error InvalidParticipant = Error.Validation(
        "SchedulingCapacity.Booking.Participant.Invalid",
        "errors.schedulingCapacity.booking.participant.invalid");

    public static readonly Error DuplicateParticipant = Error.Conflict(
        "SchedulingCapacity.Booking.Participant.Duplicate",
        "errors.schedulingCapacity.booking.participant.duplicate");

    public static readonly Error ResourcesRequired = Error.Validation(
        "SchedulingCapacity.Booking.Resources.Required",
        "errors.schedulingCapacity.booking.resources.required");

    public static readonly Error ConflictCheckRequired = Error.Conflict(
        "SchedulingCapacity.Booking.ConflictCheck.Required",
        "errors.schedulingCapacity.booking.conflictCheck.required");

    public static readonly Error ResourceConflict = Error.Conflict(
        "SchedulingCapacity.Booking.Resource.Conflict",
        "errors.schedulingCapacity.booking.resource.conflict");

    public static readonly Error ReservationNotAllowed = Error.Conflict(
        "SchedulingCapacity.Booking.Reservation.NotAllowed",
        "errors.schedulingCapacity.booking.reservation.notAllowed");

    public static readonly Error ConfirmationNotAllowed = Error.Conflict(
        "SchedulingCapacity.Booking.Confirmation.NotAllowed",
        "errors.schedulingCapacity.booking.confirmation.notAllowed");

    public static readonly Error CancellationNotAllowed = Error.Conflict(
        "SchedulingCapacity.Booking.Cancellation.NotAllowed",
        "errors.schedulingCapacity.booking.cancellation.notAllowed");


    public static readonly Error InvalidRescheduleOperation = Error.Validation(
        "SchedulingCapacity.Booking.Reschedule.Operation.Invalid",
        "errors.schedulingCapacity.booking.reschedule.operation.invalid");

    public static readonly Error InvalidRescheduleReason = Error.Validation(
        "SchedulingCapacity.Booking.Reschedule.Reason.Invalid",
        "errors.schedulingCapacity.booking.reschedule.reason.invalid");

    public static readonly Error RescheduleOperationConflict = Error.Conflict(
        "SchedulingCapacity.Booking.Reschedule.Operation.Conflict",
        "errors.schedulingCapacity.booking.reschedule.operation.conflict");

    public static readonly Error InvalidCancellationOperation = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.Operation.Invalid",
        "errors.schedulingCapacity.booking.cancellation.operation.invalid");

    public static readonly Error InvalidCancellationInitiator = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.Initiator.Invalid",
        "errors.schedulingCapacity.booking.cancellation.initiator.invalid");

    public static readonly Error InvalidCancellationReason = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.Reason.Invalid",
        "errors.schedulingCapacity.booking.cancellation.reason.invalid");

    public static readonly Error CancellationReasonDetailsRequired = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.ReasonDetails.Required",
        "errors.schedulingCapacity.booking.cancellation.reasonDetails.required");

    public static readonly Error InvalidCancellationPolicy = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.Policy.Invalid",
        "errors.schedulingCapacity.booking.cancellation.policy.invalid");

    public static readonly Error InvalidCancellationDecision = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.Decision.Invalid",
        "errors.schedulingCapacity.booking.cancellation.decision.invalid");

    public static readonly Error CancellationAfterStartNotAllowed = Error.Conflict(
        "SchedulingCapacity.Booking.Cancellation.AfterStart.NotAllowed",
        "errors.schedulingCapacity.booking.cancellation.afterStart.notAllowed");

    public static readonly Error CancellationOperationConflict = Error.Conflict(
        "SchedulingCapacity.Booking.Cancellation.Operation.Conflict",
        "errors.schedulingCapacity.booking.cancellation.operation.conflict");

    public static readonly Error CancellationOverrideReasonRequired = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.OverrideReason.Required",
        "errors.schedulingCapacity.booking.cancellation.overrideReason.required");

    public static readonly Error InvalidCancellationOverride = Error.Validation(
        "SchedulingCapacity.Booking.Cancellation.Override.Invalid",
        "errors.schedulingCapacity.booking.cancellation.override.invalid");


    public static readonly Error InvalidAttendanceOperation = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.Operation.Invalid",
        "errors.schedulingCapacity.booking.attendance.operation.invalid");

    public static readonly Error InvalidAttendance = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.Invalid",
        "errors.schedulingCapacity.booking.attendance.invalid");

    public static readonly Error AttendanceNotAllowed = Error.Conflict(
        "SchedulingCapacity.Booking.Attendance.NotAllowed",
        "errors.schedulingCapacity.booking.attendance.notAllowed");

    public static readonly Error AttendanceTooEarly = Error.Conflict(
        "SchedulingCapacity.Booking.Attendance.TooEarly",
        "errors.schedulingCapacity.booking.attendance.tooEarly");

    public static readonly Error AttendanceDelayRequired = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.Delay.Required",
        "errors.schedulingCapacity.booking.attendance.delay.required");

    public static readonly Error ArrivalTimeRequired = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.Arrival.Required",
        "errors.schedulingCapacity.booking.attendance.arrival.required");

    public static readonly Error InvalidActualPeriod = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.ActualPeriod.Invalid",
        "errors.schedulingCapacity.booking.attendance.actualPeriod.invalid");

    public static readonly Error InvalidAttendanceReason = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.Reason.Invalid",
        "errors.schedulingCapacity.booking.attendance.reason.invalid");

    public static readonly Error AttendanceOverrideReasonRequired = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.OverrideReason.Required",
        "errors.schedulingCapacity.booking.attendance.overrideReason.required");

    public static readonly Error InvalidAttendanceOverride = Error.Validation(
        "SchedulingCapacity.Booking.Attendance.Override.Invalid",
        "errors.schedulingCapacity.booking.attendance.override.invalid");

    public static readonly Error AttendanceOperationConflict = Error.Conflict(
        "SchedulingCapacity.Booking.Attendance.Operation.Conflict",
        "errors.schedulingCapacity.booking.attendance.operation.conflict");

    public static readonly Error AttendanceCorrectionWindowExpired = Error.Conflict(
        "SchedulingCapacity.Booking.Attendance.CorrectionWindow.Expired",
        "errors.schedulingCapacity.booking.attendance.correctionWindow.expired");

    public static readonly Error InvalidInstructorReplacement = Error.Validation("SchedulingCapacity.Booking.InstructorReplacement.Invalid", "errors.schedulingCapacity.booking.instructorReplacement.invalid");
    public static readonly Error InstructorReplacementNotAllowed = Error.Conflict("SchedulingCapacity.Booking.InstructorReplacement.NotAllowed", "errors.schedulingCapacity.booking.instructorReplacement.notAllowed");
    public static readonly Error InvalidInstructorReplacementReason = Error.Validation("SchedulingCapacity.Booking.InstructorReplacement.Reason.Invalid", "errors.schedulingCapacity.booking.instructorReplacement.reason.invalid");
    public static readonly Error InvalidInstructorReplacementAccessExpiry = Error.Validation("SchedulingCapacity.Booking.InstructorReplacement.AccessExpiry.Invalid", "errors.schedulingCapacity.booking.instructorReplacement.accessExpiry.invalid");
    public static readonly Error InstructorReplacementIdempotencyConflict = Error.Conflict("SchedulingCapacity.Booking.InstructorReplacement.IdempotencyConflict", "errors.schedulingCapacity.booking.instructorReplacement.idempotencyConflict");
    public static readonly Error PreviousInstructorResourceNotFound = Error.NotFound("SchedulingCapacity.Booking.InstructorReplacement.PreviousResourceNotFound", "errors.schedulingCapacity.booking.instructorReplacement.previousResourceNotFound");
    public static readonly Error InvalidVehicleReplacement = Error.Validation("SchedulingCapacity.Booking.VehicleReplacement.Invalid", "errors.schedulingCapacity.booking.vehicleReplacement.invalid");
    public static readonly Error VehicleReplacementNotAllowed = Error.Conflict("SchedulingCapacity.Booking.VehicleReplacement.NotAllowed", "errors.schedulingCapacity.booking.vehicleReplacement.notAllowed");
    public static readonly Error InvalidVehicleReplacementReason = Error.Validation("SchedulingCapacity.Booking.VehicleReplacement.Reason.Invalid", "errors.schedulingCapacity.booking.vehicleReplacement.reason.invalid");
    public static readonly Error VehicleReplacementIdempotencyConflict = Error.Conflict("SchedulingCapacity.Booking.VehicleReplacement.IdempotencyConflict", "errors.schedulingCapacity.booking.vehicleReplacement.idempotencyConflict");
    public static readonly Error PreviousVehicleResourceNotFound = Error.NotFound("SchedulingCapacity.Booking.VehicleReplacement.PreviousResourceNotFound", "errors.schedulingCapacity.booking.vehicleReplacement.previousResourceNotFound");
    public static readonly Error VehicleReplacementCompatibilityNotVerified = Error.Conflict("SchedulingCapacity.Booking.VehicleReplacement.CompatibilityNotVerified", "errors.schedulingCapacity.booking.vehicleReplacement.compatibilityNotVerified");

}
