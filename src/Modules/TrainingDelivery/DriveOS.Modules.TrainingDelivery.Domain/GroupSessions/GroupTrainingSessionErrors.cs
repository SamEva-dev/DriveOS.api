using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;

public static class GroupTrainingSessionErrors
{
    public static readonly Error NotFound = Error.NotFound("TrainingDelivery.GroupSession.NotFound", "errors.trainingDelivery.groupSession.notFound");
    public static readonly Error SourceBookingNotFound = Error.NotFound("TrainingDelivery.GroupSession.SourceBooking.NotFound", "errors.trainingDelivery.groupSession.sourceBooking.notFound");
    public static readonly Error SourceBookingNotConfirmed = Error.Conflict("TrainingDelivery.GroupSession.SourceBooking.NotConfirmed", "errors.trainingDelivery.groupSession.sourceBooking.notConfirmed");
    public static readonly Error SourceBookingWrongType = Error.Conflict("TrainingDelivery.GroupSession.SourceBooking.WrongType", "errors.trainingDelivery.groupSession.sourceBooking.wrongType");
    public static readonly Error SourceBookingIncomplete = Error.Conflict("TrainingDelivery.GroupSession.SourceBooking.Incomplete", "errors.trainingDelivery.groupSession.sourceBooking.incomplete");
    public static readonly Error CapacityExceeded = Error.Conflict("TrainingDelivery.GroupSession.Capacity.Exceeded", "errors.trainingDelivery.groupSession.capacity.exceeded");
    public static readonly Error ParticipantAlreadyExists = Error.Conflict("TrainingDelivery.GroupSession.Participant.AlreadyExists", "errors.trainingDelivery.groupSession.participant.alreadyExists");
    public static readonly Error ParticipantNotFound = Error.NotFound("TrainingDelivery.GroupSession.Participant.NotFound", "errors.trainingDelivery.groupSession.participant.notFound");
    public static readonly Error InvalidAttendance = Error.Validation("TrainingDelivery.GroupSession.Attendance.Invalid", "errors.trainingDelivery.groupSession.attendance.invalid");
    public static readonly Error InvalidAssessment = Error.Validation("TrainingDelivery.GroupSession.Assessment.Invalid", "errors.trainingDelivery.groupSession.assessment.invalid");
    public static readonly Error InvalidReport = Error.Validation("TrainingDelivery.GroupSession.Report.Invalid", "errors.trainingDelivery.groupSession.report.invalid");
    public static readonly Error OperationConflict = Error.Conflict("TrainingDelivery.GroupSession.Operation.Conflict", "errors.trainingDelivery.groupSession.operation.conflict");
}
