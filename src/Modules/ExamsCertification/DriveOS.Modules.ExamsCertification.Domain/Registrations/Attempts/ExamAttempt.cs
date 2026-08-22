using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;

/// <summary>
/// One concrete presentation to an examination. Registration is administrative intent; this aggregate owns the
/// actual attempt and its append-only exam-day timeline. It snapshots the confirmed preparation revision and
/// convocation version so stale preparation can never silently authorize the day-of-exam workflow.
/// </summary>
public sealed class ExamAttempt : AggregateRoot<ExamAttemptId>, IAuditableEntity
{
    private readonly List<ExamAttemptTimelineEntry> _timeline = [];
    private ExamAttempt() { }

    private ExamAttempt(ExamAttemptId id, OrganizationId organizationId, ExamRegistrationId registrationId,
        ExamPreparationId preparationId, PersonId studentId, int attemptNumber, int preparationRevision, int convocationVersion,
        string examType, string licenseCategory, ExamCenterId examCenterId, ExamPlaceId examPlaceId,
        DateTimeOffset scheduledStartUtc, DateTimeOffset scheduledEndUtc, DateTimeOffset meetingAtUtc,
        UserId? instructorId, VehicleId? vehicleId, BookingId schedulingBookingId, UserId actor, DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId; RegistrationId = registrationId; PreparationId = preparationId; StudentId = studentId;
        AttemptNumber = attemptNumber; PreparationRevision = preparationRevision; ConvocationVersion = convocationVersion;
        ExamType = examType; LicenseCategory = licenseCategory; ExamCenterId = examCenterId; ExamPlaceId = examPlaceId;
        ScheduledStartUtc = scheduledStartUtc.ToUniversalTime(); ScheduledEndUtc = scheduledEndUtc.ToUniversalTime();
        MeetingAtUtc = meetingAtUtc.ToUniversalTime(); InstructorId = instructorId; VehicleId = vehicleId;
        SchedulingBookingId = schedulingBookingId; Status = ExamAttemptStatus.Scheduled; AttendanceStatus = ExamAttendanceStatus.Expected;
        CreatedAtUtc = now.ToUniversalTime(); CreatedByUserId = actor;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public ExamPreparationId PreparationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int AttemptNumber { get; private set; }
    public int PreparationRevision { get; private set; }
    public int ConvocationVersion { get; private set; }
    public string ExamType { get; private set; } = string.Empty;
    public string LicenseCategory { get; private set; } = string.Empty;
    public ExamCenterId ExamCenterId { get; private set; }
    public ExamPlaceId ExamPlaceId { get; private set; }
    public DateTimeOffset ScheduledStartUtc { get; private set; }
    public DateTimeOffset ScheduledEndUtc { get; private set; }
    public DateTimeOffset MeetingAtUtc { get; private set; }
    public UserId? InstructorId { get; private set; }
    public VehicleId? VehicleId { get; private set; }
    public BookingId SchedulingBookingId { get; private set; }
    public ExamAttemptStatus Status { get; private set; }
    public ExamAttendanceStatus AttendanceStatus { get; private set; }
    public DateTimeOffset? CheckedInAtUtc { get; private set; }
    public DateTimeOffset? DepartedAtUtc { get; private set; }
    public DateTimeOffset? ArrivedAtCenterAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? ReturnedAtUtc { get; private set; }
    public string? OperationalReasonCode { get; private set; }
    public string? OperationalNotes { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public IReadOnlyCollection<ExamAttemptTimelineEntry> Timeline => _timeline.AsReadOnly();

    public bool IsTerminal => Status is ExamAttemptStatus.AwaitingResult or ExamAttemptStatus.CandidateAbsent
        or ExamAttemptStatus.Postponed or ExamAttemptStatus.Cancelled or ExamAttemptStatus.Interrupted or ExamAttemptStatus.UnableToStart;

    public static Result<ExamAttempt> Create(OrganizationId organizationId, ExamRegistrationId registrationId,
        ExamPreparationId preparationId, PersonId studentId, int attemptNumber, int preparationRevision, int convocationVersion,
        string examType, string licenseCategory, ExamCenterId examCenterId, ExamPlaceId examPlaceId,
        DateTimeOffset scheduledStartUtc, DateTimeOffset scheduledEndUtc, DateTimeOffset meetingAtUtc,
        UserId? instructorId, VehicleId? vehicleId, BookingId schedulingBookingId, Guid operationId, string requestFingerprint,
        UserId actor, DateTimeOffset now)
    {
        if (organizationId.IsEmpty || registrationId.IsEmpty || preparationId.IsEmpty || studentId.IsEmpty || examCenterId.IsEmpty
            || examPlaceId.IsEmpty || schedulingBookingId.IsEmpty || actor.IsEmpty || operationId == Guid.Empty)
            return Result.Failure<ExamAttempt>(ExamAttemptErrors.InvalidIdentifier);
        if (attemptNumber <= 0 || preparationRevision <= 0 || convocationVersion <= 0 || scheduledEndUtc <= scheduledStartUtc
            || meetingAtUtc >= scheduledStartUtc || string.IsNullOrWhiteSpace(examType) || string.IsNullOrWhiteSpace(licenseCategory)
            || string.IsNullOrWhiteSpace(requestFingerprint))
            return Result.Failure<ExamAttempt>(ExamAttemptErrors.InvalidSnapshot);

        var x = new ExamAttempt(ExamAttemptId.New(), organizationId, registrationId, preparationId, studentId, attemptNumber,
            preparationRevision, convocationVersion, examType.Trim(), licenseCategory.Trim(), examCenterId, examPlaceId,
            scheduledStartUtc, scheduledEndUtc, meetingAtUtc, instructorId, vehicleId, schedulingBookingId, actor, now);
        x.Append(operationId, requestFingerprint, ExamAttemptTimelineEntryType.AttemptCreated, null, now, actor);
        x.RaiseDomainEvent(new ExamAttemptCreatedDomainEvent(x.Id, organizationId, registrationId, studentId, attemptNumber));
        return Result.Success(x);
    }

    public bool MatchesOperation(Guid operationId, string fingerprint) =>
        _timeline.Any(x => x.OperationId == operationId && string.Equals(x.RequestFingerprint, fingerprint, StringComparison.Ordinal));

    public Result CheckIn(Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status != ExamAttemptStatus.Scheduled) return Result.Failure(ExamAttemptErrors.InvalidTransition);
        AttendanceStatus = ExamAttendanceStatus.Present; CheckedInAtUtc = occurredAtUtc.ToUniversalTime(); Status = ExamAttemptStatus.CheckedIn;
        Touch(actor, occurredAtUtc); Append(operationId, fingerprint, ExamAttemptTimelineEntryType.CheckedIn, null, occurredAtUtc, actor);
        RaiseDomainEvent(new ExamCandidateCheckedInDomainEvent(Id, OrganizationId, RegistrationId, CheckedInAtUtc.Value)); return Result.Success();
    }

    public Result RecordDeparture(Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status != ExamAttemptStatus.CheckedIn) return Result.Failure(ExamAttemptErrors.InvalidTransition);
        DepartedAtUtc = occurredAtUtc.ToUniversalTime(); Status = ExamAttemptStatus.EnRoute; Touch(actor, occurredAtUtc);
        Append(operationId, fingerprint, ExamAttemptTimelineEntryType.DepartureRecorded, null, occurredAtUtc, actor); return Result.Success();
    }

    public Result RecordArrival(Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status is not (ExamAttemptStatus.CheckedIn or ExamAttemptStatus.EnRoute)) return Result.Failure(ExamAttemptErrors.InvalidTransition);
        ArrivedAtCenterAtUtc = occurredAtUtc.ToUniversalTime(); Status = ExamAttemptStatus.AtCenter; Touch(actor, occurredAtUtc);
        Append(operationId, fingerprint, ExamAttemptTimelineEntryType.ArrivalRecorded, null, occurredAtUtc, actor); return Result.Success();
    }

    public Result Start(Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status is not (ExamAttemptStatus.CheckedIn or ExamAttemptStatus.AtCenter) || AttendanceStatus != ExamAttendanceStatus.Present)
            return Result.Failure(ExamAttemptErrors.InvalidTransition);
        StartedAtUtc = occurredAtUtc.ToUniversalTime(); Status = ExamAttemptStatus.InProgress; Touch(actor, occurredAtUtc);
        Append(operationId, fingerprint, ExamAttemptTimelineEntryType.ExamStarted, null, occurredAtUtc, actor);
        RaiseDomainEvent(new ExamAttemptStartedDomainEvent(Id, OrganizationId, RegistrationId, StartedAtUtc.Value)); return Result.Success();
    }

    public Result Complete(Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status != ExamAttemptStatus.InProgress) return Result.Failure(ExamAttemptErrors.InvalidTransition);
        CompletedAtUtc = occurredAtUtc.ToUniversalTime(); Status = ExamAttemptStatus.AwaitingResult; Touch(actor, occurredAtUtc);
        Append(operationId, fingerprint, ExamAttemptTimelineEntryType.ExamCompleted, null, occurredAtUtc, actor);
        RaiseDomainEvent(new ExamAttemptCompletedDomainEvent(Id, OrganizationId, RegistrationId, CompletedAtUtc.Value)); return Result.Success();
    }

    public Result RecordReturn(Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status != ExamAttemptStatus.AwaitingResult) return Result.Failure(ExamAttemptErrors.InvalidTransition);
        ReturnedAtUtc = occurredAtUtc.ToUniversalTime(); Touch(actor, occurredAtUtc);
        Append(operationId, fingerprint, ExamAttemptTimelineEntryType.ReturnRecorded, null, occurredAtUtc, actor); return Result.Success();
    }

    public Result ReportIncident(string code, string description, Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(description)) return Result.Failure(ExamAttemptErrors.IncidentDetailsRequired);
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        Touch(actor, occurredAtUtc); Append(operationId, fingerprint, ExamAttemptTimelineEntryType.IncidentReported,
            $"[{code.Trim()}] {description.Trim()}", occurredAtUtc, actor);
        RaiseDomainEvent(new ExamAttemptIncidentReportedDomainEvent(Id, OrganizationId, RegistrationId, code.Trim())); return Result.Success();
    }

    public Result AddOperationalNote(string note, Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        if (string.IsNullOrWhiteSpace(note)) return Result.Failure(ExamAttemptErrors.NoteRequired);
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        Touch(actor, occurredAtUtc); Append(operationId, fingerprint, ExamAttemptTimelineEntryType.OperationalNoteAdded, note.Trim(), occurredAtUtc, actor); return Result.Success();
    }

    public Result RecordLocation(decimal latitude, decimal longitude, decimal? accuracyMeters, ExamAttemptLocationPurpose purpose,
        Guid operationId, string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180 || accuracyMeters is < 0) return Result.Failure(ExamAttemptErrors.InvalidLocation);
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        Touch(actor, occurredAtUtc); Append(operationId, fingerprint, ExamAttemptTimelineEntryType.LocationRecorded, null, occurredAtUtc, actor,
            latitude, longitude, accuracyMeters, purpose); return Result.Success();
    }

    public Result RecordValidatedResourceChange(UserId? instructorId, VehicleId? vehicleId, string reason, Guid operationId,
        string fingerprint, UserId actor, DateTimeOffset occurredAtUtc)
    {
        if ((!instructorId.HasValue && !vehicleId.HasValue) || string.IsNullOrWhiteSpace(reason)) return Result.Failure(ExamAttemptErrors.ResourceChangeRequired);
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (instructorId.HasValue) InstructorId = instructorId; if (vehicleId.HasValue) VehicleId = vehicleId;
        Touch(actor, occurredAtUtc); Append(operationId, fingerprint, ExamAttemptTimelineEntryType.ResourceChangeRecorded, reason.Trim(), occurredAtUtc, actor,
            instructorId: instructorId, vehicleId: vehicleId); return Result.Success();
    }

    public Result MarkAbsent(bool excused, string reasonCode, string? notes, Guid operationId, string fingerprint, UserId actor, DateTimeOffset now) =>
        TerminateBeforeStart(excused ? ExamAttendanceStatus.ExcusedAbsent : ExamAttendanceStatus.Absent, ExamAttemptStatus.CandidateAbsent,
            ExamAttemptTimelineEntryType.CandidateAbsent, reasonCode, notes, operationId, fingerprint, actor, now,
            () => RaiseDomainEvent(new ExamCandidateAbsentDomainEvent(Id, OrganizationId, RegistrationId, excused, reasonCode.Trim())));

    public Result Postpone(string reasonCode, string? notes, Guid operationId, string fingerprint, UserId actor, DateTimeOffset now) =>
        TerminateBeforeStart(null, ExamAttemptStatus.Postponed, ExamAttemptTimelineEntryType.Postponed, reasonCode, notes, operationId, fingerprint, actor, now,
            () => RaiseDomainEvent(new ExamAttemptPostponedDomainEvent(Id, OrganizationId, RegistrationId, reasonCode.Trim())));

    public Result Cancel(string reasonCode, string? notes, Guid operationId, string fingerprint, UserId actor, DateTimeOffset now) =>
        TerminateBeforeStart(null, ExamAttemptStatus.Cancelled, ExamAttemptTimelineEntryType.Cancelled, reasonCode, notes, operationId, fingerprint, actor, now,
            () => RaiseDomainEvent(new ExamAttemptCancelledDomainEvent(Id, OrganizationId, RegistrationId, reasonCode.Trim())));

    public Result UnableToStart(string reasonCode, string? notes, Guid operationId, string fingerprint, UserId actor, DateTimeOffset now) =>
        TerminateBeforeStart(null, ExamAttemptStatus.UnableToStart, ExamAttemptTimelineEntryType.UnableToStart, reasonCode, notes, operationId, fingerprint, actor, now,
            () => RaiseDomainEvent(new ExamAttemptUnableToStartDomainEvent(Id, OrganizationId, RegistrationId, reasonCode.Trim())));

    public Result Interrupt(string reasonCode, string? notes, Guid operationId, string fingerprint, UserId actor, DateTimeOffset now)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status != ExamAttemptStatus.InProgress) return Result.Failure(ExamAttemptErrors.InvalidTransition);
        if (string.IsNullOrWhiteSpace(reasonCode)) return Result.Failure(ExamAttemptErrors.ReasonRequired);
        Status = ExamAttemptStatus.Interrupted; SetReason(reasonCode, notes); CompletedAtUtc = now.ToUniversalTime(); Touch(actor, now);
        Append(operationId, fingerprint, ExamAttemptTimelineEntryType.Interrupted, OperationalReasonCode, now, actor);
        RaiseDomainEvent(new ExamAttemptInterruptedDomainEvent(Id, OrganizationId, RegistrationId, OperationalReasonCode!)); return Result.Success();
    }

    private Result TerminateBeforeStart(ExamAttendanceStatus? attendance, ExamAttemptStatus status, ExamAttemptTimelineEntryType type,
        string reasonCode, string? notes, Guid operationId, string fingerprint, UserId actor, DateTimeOffset now, Action raise)
    {
        var replay = ValidateOperation(operationId, fingerprint); if (replay.IsFailure) return replay; if (HasOperation(operationId)) return Result.Success();
        if (Status is not (ExamAttemptStatus.Scheduled or ExamAttemptStatus.CheckedIn or ExamAttemptStatus.EnRoute or ExamAttemptStatus.AtCenter))
            return Result.Failure(ExamAttemptErrors.InvalidTransition);
        if (string.IsNullOrWhiteSpace(reasonCode)) return Result.Failure(ExamAttemptErrors.ReasonRequired);
        if (attendance.HasValue) AttendanceStatus = attendance.Value; Status = status; SetReason(reasonCode, notes); Touch(actor, now);
        Append(operationId, fingerprint, type, OperationalReasonCode, now, actor); raise(); return Result.Success();
    }

    private Result ValidateOperation(Guid operationId, string fingerprint)
    {
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(fingerprint)) return Result.Failure(ExamAttemptErrors.InvalidOperation);
        var existing = _timeline.FirstOrDefault(x => x.OperationId == operationId);
        return existing is null || string.Equals(existing.RequestFingerprint, fingerprint, StringComparison.Ordinal)
            ? Result.Success() : Result.Failure(ExamAttemptErrors.OperationConflict);
    }
    private bool HasOperation(Guid operationId) => _timeline.Any(x => x.OperationId == operationId);
    private void Append(Guid operationId, string fingerprint, ExamAttemptTimelineEntryType type, string? note, DateTimeOffset occurredAtUtc,
        UserId actor, decimal? latitude = null, decimal? longitude = null, decimal? accuracyMeters = null,
        ExamAttemptLocationPurpose? purpose = null, UserId? instructorId = null, VehicleId? vehicleId = null) =>
        _timeline.Add(new ExamAttemptTimelineEntry(ExamAttemptTimelineEntryId.New(), Id, OrganizationId, operationId, fingerprint, type, Status,
            note, occurredAtUtc, actor, latitude, longitude, accuracyMeters, purpose, instructorId, vehicleId));
    private void SetReason(string code, string? notes) { OperationalReasonCode = code.Trim(); OperationalNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(); }
    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;

        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }

    private void Touch(UserId actor, DateTimeOffset now) => SetModifiedAudit(now, actor);
}
