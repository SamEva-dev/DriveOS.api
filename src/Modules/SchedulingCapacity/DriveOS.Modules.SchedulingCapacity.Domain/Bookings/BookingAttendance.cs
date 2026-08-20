using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.SchedulingCapacity.Domain.Bookings;

public sealed class BookingAttendance
{
    private BookingAttendance() { }

    internal BookingAttendance(
        BookingAttendanceId id,
        BookingId bookingId,
        Guid operationId,
        BookingAttendanceId? supersedesAttendanceId,
        AttendanceStatus status,
        DateTimeOffset recordedAtUtc,
        UserId recordedBy,
        DateTimeOffset? arrivalTimeUtc,
        DateTimeOffset? departureTimeUtc,
        int delayMinutes,
        string? reason,
        Guid? evidenceDocumentId,
        AttendanceChargeDecision chargeDecision,
        AttendanceCreditDecision creditDecision,
        AttendanceFollowUpAction followUpAction,
        bool overrideApplied,
        string? overrideReason)
    {
        Id = id;
        BookingId = bookingId;
        OperationId = operationId;
        SupersedesAttendanceId = supersedesAttendanceId;
        Status = status;
        RecordedAtUtc = recordedAtUtc.ToUniversalTime();
        RecordedBy = recordedBy;
        ArrivalTimeUtc = arrivalTimeUtc?.ToUniversalTime();
        DepartureTimeUtc = departureTimeUtc?.ToUniversalTime();
        DelayMinutes = delayMinutes;
        Reason = reason;
        EvidenceDocumentId = evidenceDocumentId;
        ChargeDecision = chargeDecision;
        CreditDecision = creditDecision;
        FollowUpAction = followUpAction;
        OverrideApplied = overrideApplied;
        OverrideReason = overrideReason;
    }

    public BookingAttendanceId Id { get; private set; }
    public BookingId BookingId { get; private set; }
    public Guid OperationId { get; private set; }
    public BookingAttendanceId? SupersedesAttendanceId { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
    public UserId RecordedBy { get; private set; }
    public DateTimeOffset? ArrivalTimeUtc { get; private set; }
    public DateTimeOffset? DepartureTimeUtc { get; private set; }
    public int DelayMinutes { get; private set; }
    public string? Reason { get; private set; }
    public Guid? EvidenceDocumentId { get; private set; }
    public AttendanceChargeDecision ChargeDecision { get; private set; }
    public AttendanceCreditDecision CreditDecision { get; private set; }
    public AttendanceFollowUpAction FollowUpAction { get; private set; }
    public bool OverrideApplied { get; private set; }
    public string? OverrideReason { get; private set; }
}
