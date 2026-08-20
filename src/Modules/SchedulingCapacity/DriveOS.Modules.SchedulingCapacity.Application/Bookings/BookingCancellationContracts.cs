using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Application.Bookings;

public sealed record BookingCancellationPolicyResolution(
    string PolicyCode,
    int PolicyVersion,
    string ExplanationKey,
    BookingCreditDecision CreditDecision,
    BookingFeeDecision FeeDecision,
    bool ReplacementRequired);

public sealed record BookingCancellationPreviewResponse(
    Guid BookingId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    int Initiator,
    int ReasonCode,
    int NoticeDurationMinutes,
    string PolicyCode,
    int PolicyVersion,
    string PolicyExplanationKey,
    int CreditDecision,
    int FeeDecision,
    bool ReplacementRequired);

public sealed record BookingCancellationResponse(
    Guid Id,
    Guid OperationId,
    int Initiator,
    Guid? InitiatorId,
    int ReasonCode,
    string? ReasonDetails,
    DateTimeOffset CancelledAtUtc,
    int NoticeDurationMinutes,
    string PolicyCode,
    int PolicyVersion,
    string PolicyExplanationKey,
    int CreditDecision,
    int FeeDecision,
    int NotificationDecision,
    bool ReplacementRequired,
    bool OverrideApplied,
    string? OverrideReason);

public interface IBookingCancellationPolicyGateway
{
    Task<BookingCancellationPolicyResolution> ResolveAsync(
        OrganizationId organizationId,
        Booking booking,
        CancellationInitiator initiator,
        CancellationReasonCode reasonCode,
        DateTimeOffset cancelledAtUtc,
        CancellationToken cancellationToken = default);
}
