using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingCancellation
{
    private BookingCancellation() { }

    internal BookingCancellation(
        BookingCancellationId id,
        BookingId bookingId,
        Guid operationId,
        CancellationInitiator initiator,
        Guid? initiatorId,
        CancellationReasonCode reasonCode,
        string? reasonDetails,
        DateTimeOffset cancelledAtUtc,
        int noticeDurationMinutes,
        string policyCode,
        int policyVersion,
        string policyExplanationKey,
        BookingCreditDecision creditDecision,
        BookingFeeDecision feeDecision,
        BookingNotificationDecision notificationDecision,
        bool replacementRequired,
        bool overrideApplied,
        string? overrideReason)
    {
        Id = id;
        BookingId = bookingId;
        OperationId = operationId;
        Initiator = initiator;
        InitiatorId = initiatorId;
        ReasonCode = reasonCode;
        ReasonDetails = reasonDetails;
        CancelledAtUtc = cancelledAtUtc.ToUniversalTime();
        NoticeDurationMinutes = noticeDurationMinutes;
        PolicyCode = policyCode;
        PolicyVersion = policyVersion;
        PolicyExplanationKey = policyExplanationKey;
        CreditDecision = creditDecision;
        FeeDecision = feeDecision;
        NotificationDecision = notificationDecision;
        ReplacementRequired = replacementRequired;
        OverrideApplied = overrideApplied;
        OverrideReason = overrideReason;
    }

    public BookingCancellationId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public Guid OperationId { get; private set; }
    public CancellationInitiator Initiator { get; private set; }
    public Guid? InitiatorId { get; private set; }
    public CancellationReasonCode ReasonCode { get; private set; }
    public string? ReasonDetails { get; private set; }
    public DateTimeOffset CancelledAtUtc { get; private set; }
    public int NoticeDurationMinutes { get; private set; }
    public string PolicyCode { get; private set; } = string.Empty;
    public int PolicyVersion { get; private set; }
    public string PolicyExplanationKey { get; private set; } = string.Empty;
    public BookingCreditDecision CreditDecision { get; private set; }
    public BookingFeeDecision FeeDecision { get; private set; }
    public BookingNotificationDecision NotificationDecision { get; private set; }
    public bool ReplacementRequired { get; private set; }
    public bool OverrideApplied { get; private set; }
    public string? OverrideReason { get; private set; }
}
