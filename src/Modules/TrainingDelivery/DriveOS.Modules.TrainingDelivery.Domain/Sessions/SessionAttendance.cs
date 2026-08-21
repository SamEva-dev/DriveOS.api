using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

public sealed class SessionAttendance : Entity<TrainingSessionAttendanceId>
{
    private SessionAttendance() { }

    private SessionAttendance(
        TrainingSessionAttendanceId id,
        TrainingSessionId trainingSessionId,
        Guid operationId,
        string requestFingerprint,
        int revision,
        TrainingSessionAttendanceStatus status,
        DateTimeOffset? actualArrivalAtUtc,
        DateTimeOffset? actualDepartureAtUtc,
        int lateMinutes,
        string? reason,
        Guid? evidenceDocumentId,
        UserId recordedByUserId,
        DateTimeOffset recordedAtUtc,
        TrainingSessionAttendanceId? supersedesAttendanceId,
        bool isOverride,
        string? overrideReason)
        : base(id)
    {
        TrainingSessionId = trainingSessionId;
        OperationId = operationId;
        RequestFingerprint = requestFingerprint;
        Revision = revision;
        Status = status;
        ActualArrivalAtUtc = actualArrivalAtUtc;
        ActualDepartureAtUtc = actualDepartureAtUtc;
        LateMinutes = lateMinutes;
        Reason = reason;
        EvidenceDocumentId = evidenceDocumentId;
        RecordedByUserId = recordedByUserId;
        RecordedAtUtc = recordedAtUtc;
        SupersedesAttendanceId = supersedesAttendanceId;
        IsOverride = isOverride;
        OverrideReason = overrideReason;
    }

    public TrainingSessionId TrainingSessionId { get; private set; }
    public Guid OperationId { get; private set; }
    public string RequestFingerprint { get; private set; } = string.Empty;
    public int Revision { get; private set; }
    public TrainingSessionAttendanceStatus Status { get; private set; }
    public DateTimeOffset? ActualArrivalAtUtc { get; private set; }
    public DateTimeOffset? ActualDepartureAtUtc { get; private set; }
    public int LateMinutes { get; private set; }
    public string? Reason { get; private set; }
    public Guid? EvidenceDocumentId { get; private set; }
    public UserId RecordedByUserId { get; private set; }
    public DateTimeOffset RecordedAtUtc { get; private set; }
    public TrainingSessionAttendanceId? SupersedesAttendanceId { get; private set; }
    public bool IsOverride { get; private set; }
    public string? OverrideReason { get; private set; }

    internal static Result<SessionAttendance> Create(
        TrainingSessionAttendanceId id,
        TrainingSessionId trainingSessionId,
        Guid operationId,
        string requestFingerprint,
        int revision,
        TrainingSessionAttendanceStatus status,
        DateTimeOffset? actualArrivalAtUtc,
        DateTimeOffset? actualDepartureAtUtc,
        int lateMinutes,
        string? reason,
        Guid? evidenceDocumentId,
        UserId recordedByUserId,
        DateTimeOffset recordedAtUtc,
        TrainingSessionAttendanceId? supersedesAttendanceId,
        bool isOverride,
        string? overrideReason)
    {
        if (id.IsEmpty || trainingSessionId.IsEmpty || operationId == Guid.Empty || string.IsNullOrWhiteSpace(requestFingerprint) || recordedByUserId.IsEmpty || revision < 1)
            return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalid);
        if (!Enum.IsDefined(status)) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalidStatus);
        if (lateMinutes < 0 || lateMinutes > 24 * 60) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalidLateMinutes);
        if (actualArrivalAtUtc.HasValue && actualDepartureAtUtc.HasValue && actualDepartureAtUtc <= actualArrivalAtUtc)
            return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalidActualPeriod);

        string? normalizedReason = Normalize(reason, 2000);
        if (reason is not null && normalizedReason is null) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceReasonTooLong);
        string? normalizedOverride = Normalize(overrideReason, 2000);
        if (overrideReason is not null && normalizedOverride is null) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceOverrideReasonTooLong);
        if (isOverride && string.IsNullOrWhiteSpace(normalizedOverride)) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceOverrideReasonRequired);

        return Result.Success(new SessionAttendance(
            id,
            trainingSessionId,
            operationId,
            requestFingerprint,
            revision,
            status,
            actualArrivalAtUtc?.ToUniversalTime(),
            actualDepartureAtUtc?.ToUniversalTime(),
            lateMinutes,
            normalizedReason,
            evidenceDocumentId,
            recordedByUserId,
            recordedAtUtc.ToUniversalTime(),
            supersedesAttendanceId,
            isOverride,
            normalizedOverride));
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        string normalized = value.Trim();
        return normalized.Length <= maxLength ? normalized : null;
    }
}
