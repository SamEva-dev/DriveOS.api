using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public sealed record CreateBookingCommand(
    OrganizationId OrganizationId,
    string IdempotencyKey,
    BranchId? BranchId,
    int BookingType,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    string Title,
    Guid? TrainingPathId,
    string? TrainingCategory,
    string? Objectives,
    string? MeetingPoint,
    string? PricingReference,
    Guid? TrainingCreditAccountId,
    decimal? CreditQuantity,
    string? Notes,
    int NotificationPolicy,
    IReadOnlyCollection<CreateBookingResourceRequest> Resources,
    IReadOnlyCollection<CreateBookingParticipantRequest> Participants) : ICommand<BookingId>;

public sealed record CheckBookingConflictsCommand(OrganizationId OrganizationId, BookingId BookingId) : ICommand<BookingConflictCheckResponse>;
public sealed record HoldBookingSlotCommand(OrganizationId OrganizationId, BookingId BookingId, int DurationMinutes) : ICommand<BookingConflictCheckResponse>;
public sealed record ReserveBookingCommand(OrganizationId OrganizationId, BookingId BookingId) : ICommand<BookingConflictCheckResponse>;
public sealed record ConfirmBookingCommand(OrganizationId OrganizationId, BookingId BookingId, UserId ActorUserId) : ICommand<BookingConflictCheckResponse>;
public sealed record PreviewRescheduleBookingCommand(
    OrganizationId OrganizationId,
    BookingId BookingId,
    Guid OperationId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    BranchId? BranchId,
    IReadOnlyCollection<BookingRescheduleResourceRequest>? Resources,
    string Reason) : ICommand<BookingRescheduleImpactResponse>;

public sealed record RescheduleBookingCommand(
    OrganizationId OrganizationId,
    BookingId BookingId,
    Guid OperationId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    BranchId? BranchId,
    IReadOnlyCollection<BookingRescheduleResourceRequest>? Resources,
    string Reason) : ICommand<BookingRescheduleImpactResponse>;
public sealed record PreviewCancelBookingCommand(
    OrganizationId OrganizationId,
    BookingId BookingId,
    int Initiator,
    Guid? InitiatorId,
    int ReasonCode,
    string? ReasonDetails) : ICommand<BookingCancellationPreviewResponse>;

public sealed record CancelBookingCommand(
    OrganizationId OrganizationId,
    BookingId BookingId,
    Guid OperationId,
    int Initiator,
    Guid? InitiatorId,
    int ReasonCode,
    string? ReasonDetails,
    int NotificationDecision,
    bool OverrideApplied,
    string? OverrideReason) : ICommand<BookingCancellationResponse>;

public sealed record RecordBookingAttendanceCommand(
    OrganizationId OrganizationId,
    BookingId BookingId,
    Guid OperationId,
    int Status,
    DateTimeOffset? ArrivalTimeUtc,
    DateTimeOffset? DepartureTimeUtc,
    int DelayMinutes,
    string? Reason,
    Guid? EvidenceDocumentId,
    int FollowUpAction) : ICommand<BookingAttendanceResponse>;

public sealed record CorrectBookingAttendanceCommand(
    OrganizationId OrganizationId,
    BookingId BookingId,
    Guid OperationId,
    int Status,
    DateTimeOffset? ArrivalTimeUtc,
    DateTimeOffset? DepartureTimeUtc,
    int DelayMinutes,
    string? Reason,
    Guid? EvidenceDocumentId,
    int FollowUpAction,
    bool OverrideApplied,
    string? OverrideReason) : ICommand<BookingAttendanceResponse>;
