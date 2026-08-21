using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public sealed record CreateBookingResourceRequest(Guid CalendarResourceId, int Quantity);
public sealed record CreateBookingParticipantRequest(int ParticipantType, Guid ExternalParticipantId);

public sealed record BookingResourceResponse(Guid Id, Guid CalendarResourceId, int Quantity);
public sealed record BookingParticipantResponse(Guid Id, int ParticipantType, Guid ExternalParticipantId);
public sealed record BookingResponse(
    Guid Id,
    Guid OrganizationId,
    Guid? BranchId,
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
    int CreditReservationStatus,
    string? CreditReservationReference,
    string? Notes,
    int NotificationPolicy,
    DateTimeOffset? HoldExpiresAtUtc,
    int Status,
    string? CancellationReason,
    IReadOnlyCollection<BookingResourceResponse> Resources,
    IReadOnlyCollection<BookingParticipantResponse> Participants,
    IReadOnlyCollection<BookingRescheduleHistoryResponse> RescheduleHistory,
    BookingCancellationResponse? Cancellation,
    BookingAttendanceResponse? Attendance,
    IReadOnlyCollection<BookingAttendanceResponse> AttendanceHistory,
    IReadOnlyCollection<BookingInstructorReplacementResponse> InstructorReplacementHistory,
    IReadOnlyCollection<BookingVehicleReplacementResponse> VehicleReplacementHistory);

public sealed record BookingConflictResponse(
    int Type,
    Guid CalendarResourceId,
    Guid? ConflictingBookingId,
    int RequestedQuantity,
    int AvailableCapacity,
    string? Reason);

public sealed record BookingConflictCheckResponse(
    Guid BookingId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    bool IsConflictFree,
    IReadOnlyCollection<BookingConflictResponse> Conflicts);

public interface IBookingReadService
{
    Task<BookingResponse?> GetAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BookingResponse>> ListAsync(OrganizationId organizationId, BranchId? branchId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken = default);
}

public interface IBookingConflictAssessmentService
{
    Task<BookingConflictAssessment> AssessAsync(Booking booking, CancellationToken cancellationToken = default);
}

public interface IBookingCapacityLock
{
    Task AcquireAsync(
        OrganizationId organizationId,
        IReadOnlyCollection<CalendarResourceId> resourceIds,
        CancellationToken cancellationToken = default);
}

public sealed record BookingAttendanceResponse(
    Guid Id,
    Guid OperationId,
    Guid? SupersedesAttendanceId,
    int Status,
    DateTimeOffset RecordedAtUtc,
    Guid RecordedBy,
    DateTimeOffset? ArrivalTimeUtc,
    DateTimeOffset? DepartureTimeUtc,
    int DelayMinutes,
    string? Reason,
    Guid? EvidenceDocumentId,
    int ChargeDecision,
    int CreditDecision,
    int FollowUpAction,
    bool OverrideApplied,
    string? OverrideReason);

public sealed record BookingInstructorReplacementResponse(Guid Id, Guid OperationId, Guid PreviousInstructorId, Guid ReplacementInstructorId, Guid PreviousResourceId, Guid ReplacementResourceId, int Mode, string Reason, DateTimeOffset OccurredAtUtc, DateTimeOffset? AccessExpiresAtUtc);
public sealed record BookingVehicleReplacementResponse(Guid Id, Guid OperationId, Guid PreviousVehicleId, Guid ReplacementVehicleId, Guid PreviousResourceId, Guid ReplacementResourceId, int Mode, string Reason, DateTimeOffset OccurredAtUtc);

public sealed record BookingCreditReservationResult(string Reference);

public interface IBookingCreditReservationGateway
{
    Task<Result<BookingCreditReservationResult>> ReserveAsync(
        OrganizationId organizationId,
        Guid trainingCreditAccountId,
        decimal quantity,
        BookingId bookingId,
        UserId actorUserId,
        CancellationToken cancellationToken = default);
}

public interface IBookingCreationIdempotencyLock
{
    Task AcquireAsync(
        OrganizationId organizationId,
        string idempotencyKey,
        CancellationToken cancellationToken = default);
}

public sealed record BookingExecutionReadinessResponse(
    bool Exists,
    bool IsConfirmed,
    bool IsConflictFree,
    IReadOnlyCollection<BookingConflictResponse> Conflicts)
{
    public bool IsReady => Exists && IsConfirmed && IsConflictFree;
}

public interface IBookingExecutionReadinessService
{
    Task<BookingExecutionReadinessResponse> CheckAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default);
}
