using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Attempts;

public sealed record CreateExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record GetExamAttemptQuery(OrganizationId OrganizationId, ExamRegistrationId RegistrationId) : IQuery<ExamAttemptResponse>;
public sealed record CheckInExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record RecordExamDepartureCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record RecordExamArrivalCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record StartExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record CompleteExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record RecordExamReturnCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record ReportExamAttemptIncidentCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string IncidentCode, string Description, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record AddExamAttemptNoteCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string Note, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record RecordExamAttemptLocationCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, decimal Latitude, decimal Longitude, decimal? AccuracyMeters, string Purpose, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record RecordExamAttemptResourceChangeCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string Reason, Guid OperationId, DateTimeOffset? OccurredAtUtc, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record MarkExamAttemptAbsentCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, bool Excused, string ReasonCode, string? Notes, Guid OperationId, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record PostponeExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode, string? Notes, Guid OperationId, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record CancelExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode, string? Notes, Guid OperationId, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record InterruptExamAttemptCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode, string? Notes, Guid OperationId, UserId ActorUserId) : ICommand<ExamAttemptResponse>;
public sealed record MarkExamAttemptUnableToStartCommand(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, string ReasonCode, string? Notes, Guid OperationId, UserId ActorUserId) : ICommand<ExamAttemptResponse>;

public sealed record ExamAttemptTimelineResponse(Guid Id, Guid OperationId, string Type, string Status, string? Note, DateTimeOffset OccurredAtUtc,
    Guid ActorUserId, decimal? Latitude, decimal? Longitude, decimal? AccuracyMeters, string? LocationPurpose, Guid? InstructorId, Guid? VehicleId);

public sealed record ExamAttemptResponse(Guid Id, Guid RegistrationId, Guid PreparationId, Guid StudentId, int AttemptNumber,
    int PreparationRevision, int ConvocationVersion, string ExamType, string LicenseCategory, Guid ExamCenterId, Guid ExamPlaceId,
    DateTimeOffset ScheduledStartUtc, DateTimeOffset ScheduledEndUtc, DateTimeOffset MeetingAtUtc, Guid? InstructorId, Guid? VehicleId,
    Guid SchedulingBookingId, string Status, string AttendanceStatus, DateTimeOffset? CheckedInAtUtc, DateTimeOffset? DepartedAtUtc,
    DateTimeOffset? ArrivedAtCenterAtUtc, DateTimeOffset? StartedAtUtc, DateTimeOffset? CompletedAtUtc, DateTimeOffset? ReturnedAtUtc,
    string? OperationalReasonCode, string? OperationalNotes, IReadOnlyCollection<ExamAttemptTimelineResponse> Timeline,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? LastModifiedAtUtc);
