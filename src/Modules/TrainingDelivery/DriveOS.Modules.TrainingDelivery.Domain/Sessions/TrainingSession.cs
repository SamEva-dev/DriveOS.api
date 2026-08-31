using DriveOS.Modules.TrainingDelivery.Domain.Sessions.Events;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Domain.Sessions;

/// <summary>
/// Aggregate root representing the pedagogical session effectively executed from a confirmed Scheduling booking.
/// It preserves the planning snapshot received from BC-09 while owning the execution lifecycle, readiness audit and actual resources/timestamps used by BC-10.
/// </summary>
public sealed class TrainingSession : AggregateRoot<TrainingSessionId>, IAuditableEntity
{
    private readonly List<SessionAttendance> _attendanceHistory = [];
    private readonly List<SessionIntervention> _interventions = [];
    private readonly List<SessionObservation> _observations = [];
    private readonly List<SessionMarker> _markers = [];
    private readonly List<SessionInterruption> _interruptions = [];
    private readonly List<SessionOdometerReading> _odometerReadings = [];
    private readonly List<SessionEnergyEntry> _energyEntries = [];
    private readonly List<SessionCompetencyAssessment> _competencyAssessments = [];
    private SessionReport? _report;

    private TrainingSession() { }

    private TrainingSession(TrainingSessionId id, TrainingSessionMaterialization materialization) : base(id)
    {
        OrganizationId = materialization.OrganizationId;
        StudentOwnerOrganizationId = materialization.StudentOwnerOrganizationId;
        PerformingOrganizationId = materialization.PerformingOrganizationId;
        SourceBookingId = materialization.SourceBookingId;
        StudentId = materialization.StudentId;
        TrainingPathId = materialization.TrainingPathId;
        InstructorId = materialization.InstructorId;
        BranchId = materialization.BranchId;
        VehicleId = materialization.VehicleId;
        PlannedStartAtUtc = materialization.PlannedStartAtUtc.ToUniversalTime();
        PlannedEndAtUtc = materialization.PlannedEndAtUtc.ToUniversalTime();
        TrainingCategory = materialization.TrainingCategory;
        Objectives = materialization.Objectives;
        MeetingPoint = materialization.MeetingPoint;
        PricingReference = materialization.PricingReference;
        TrainingCreditAccountId = materialization.TrainingCreditAccountId;
        CreditQuantity = materialization.CreditQuantity;
        CreditReservationReference = materialization.CreditReservationReference;
        Status = TrainingSessionStatus.Scheduled;
    }

    /// <summary>Tenant organization that owns this Training Delivery aggregate and scopes every read/write operation.</summary>
    public OrganizationId OrganizationId { get; private set; }
    /// <summary>Organization owning the student's file, even when another organization performs the session.</summary>
    public OrganizationId StudentOwnerOrganizationId { get; private set; }
    /// <summary>Organization expected to perform the service; it may differ from the student-file owner for partner/freelance delivery.</summary>
    public OrganizationId PerformingOrganizationId { get; private set; }
    /// <summary>Scheduling booking that originated the session and acts as the idempotency/correlation anchor between BC-09 and BC-10.</summary>
    public BookingId SourceBookingId { get; private set; }
    /// <summary>Student for whom the training service is delivered.</summary>
    public PersonId StudentId { get; private set; }
    /// <summary>Pedagogical training path to which the executed session contributes.</summary>
    public TrainingPathId TrainingPathId { get; private set; }
    /// <summary>Instructor planned when the session was materialized. This remains a planning snapshot and is not overwritten by actual execution data.</summary>
    public UserId InstructorId { get; private set; }
    /// <summary>Branch planned when the session was materialized.</summary>
    public BranchId? BranchId { get; private set; }
    /// <summary>Vehicle planned when the session was materialized, when applicable. Fleet remains the authoritative owner of vehicle compliance data.</summary>
    public Guid? VehicleId { get; private set; }
    /// <summary>Original UTC start instant copied from Scheduling when the TrainingSession was materialized.</summary>
    public DateTimeOffset PlannedStartAtUtc { get; private set; }
    /// <summary>Original UTC end instant copied from Scheduling when the TrainingSession was materialized.</summary>
    public DateTimeOffset PlannedEndAtUtc { get; private set; }
    /// <summary>Training/licence category snapshot used to preserve the eligibility context of the planned service.</summary>
    public string? TrainingCategory { get; private set; }
    /// <summary>Pedagogical objectives planned for the session at materialization time.</summary>
    public string? Objectives { get; private set; }
    /// <summary>Meeting point planned for the session at materialization time.</summary>
    public string? MeetingPoint { get; private set; }
    /// <summary>Pricing reference snapshot used later to correlate delivered service and billing without letting BC-10 own pricing rules.</summary>
    public string? PricingReference { get; private set; }
    /// <summary>Funding credit account referenced by the confirmed booking, when applicable.</summary>
    public TrainingCreditAccountId? TrainingCreditAccountId { get; private set; }
    /// <summary>Quantity of credit reserved for the booking, when applicable.</summary>
    public decimal? CreditQuantity { get; private set; }
    /// <summary>Idempotent Funding reservation reference that later prevents double consumption when the session is completed.</summary>
    public string? CreditReservationReference { get; private set; }
    /// <summary>Current Training Delivery execution lifecycle state.</summary>
    public TrainingSessionStatus Status { get; private set; }
    /// <summary>UTC instant of the latest successful readiness validation against current Scheduling/resource state.</summary>
    public DateTimeOffset? ReadinessCheckedAtUtc { get; private set; }
    /// <summary>Authenticated user who performed the latest successful readiness validation.</summary>
    public UserId? ReadinessCheckedByUserId { get; private set; }
    /// <summary>Instructor that Scheduling considered current and valid at the latest readiness validation.</summary>
    public UserId? ReadyInstructorId { get; private set; }
    /// <summary>Vehicle that Scheduling considered current at readiness time, which can differ from the original materialization snapshot after a replacement.</summary>
    public Guid? ReadyVehicleId { get; private set; }
    /// <summary>Branch considered current at readiness time.</summary>
    public BranchId? ReadyBranchId { get; private set; }
    /// <summary>Current planned start returned by Scheduling at readiness time; it preserves visibility when the source booking was rescheduled after materialization.</summary>
    public DateTimeOffset? ReadyPlannedStartAtUtc { get; private set; }
    /// <summary>Current planned end returned by Scheduling at readiness time.</summary>
    public DateTimeOffset? ReadyPlannedEndAtUtc { get; private set; }
    /// <summary>Actual instructor used when the session starts. It is separate from the original planned instructor snapshot.</summary>
    public UserId? ActualInstructorId { get; private set; }
    /// <summary>Actual vehicle used when the session starts, when applicable.</summary>
    public Guid? ActualVehicleId { get; private set; }
    /// <summary>Actual branch/context used when the session starts.</summary>
    public BranchId? ActualBranchId { get; private set; }
    /// <summary>Actual UTC start instant owned by Training Delivery and never inferred from the planned period.</summary>
    public DateTimeOffset? ActualStartAtUtc { get; private set; }
    /// <summary>Authenticated user who started the session.</summary>
    public UserId? StartedByUserId { get; private set; }
    /// <summary>Idempotency key of the accepted start operation, persisted for offline/retry replay.</summary>
    public Guid? StartOperationId { get; private set; }
    /// <summary>Fingerprint of the accepted start request. Reusing the same operation id with another payload is rejected.</summary>
    public string? StartRequestFingerprint { get; private set; }
    /// <summary>Identifier of the authoritative current attendance record owned by Training Delivery. Older revisions remain in <see cref="AttendanceHistory"/>.</summary>
    public TrainingSessionAttendanceId? CurrentAttendanceId { get; private set; }
    /// <summary>Append-only attendance history. Corrections create a new revision instead of mutating or deleting the previous observation.</summary>
    public IReadOnlyCollection<SessionAttendance> AttendanceHistory => _attendanceHistory.AsReadOnly();
    /// <summary>Append-only record of pedagogical and safety interventions performed during the effective session.</summary>
    public IReadOnlyCollection<SessionIntervention> Interventions => _interventions.AsReadOnly();
    /// <summary>Append-only operational observations captured during the lesson; these are distinct from formal competency assessments owned by Pedagogy.</summary>
    public IReadOnlyCollection<SessionObservation> Observations => _observations.AsReadOnly();
    /// <summary>Append-only field markers captured in a few seconds during the lesson and later reusable when composing the pedagogical report.</summary>
    public IReadOnlyCollection<SessionMarker> Markers => _markers.AsReadOnly();
    /// <summary>History of operational interruptions and resumptions. Every interruption keeps its cause, actors and actual timestamps.</summary>
    public IReadOnlyCollection<SessionInterruption> Interruptions => _interruptions.AsReadOnly();
    /// <summary>Ordered odometer readings observed during this session. Fleet remains authoritative for the vehicle's global odometer.</summary>
    public IReadOnlyCollection<SessionOdometerReading> OdometerReadings => _odometerReadings.AsReadOnly();
    /// <summary>Append-only energy, refuelling and charging observations captured for the vehicle used by this session. Fleet remains authoritative for the global vehicle history.</summary>
    public IReadOnlyCollection<SessionEnergyEntry> EnergyEntries => _energyEntries.AsReadOnly();
    /// <summary>Session-level snapshots of competency assessments recorded from the real lesson. Curriculum & Pedagogy remains authoritative for the consolidated competency level and progression.</summary>
    public IReadOnlyCollection<SessionCompetencyAssessment> CompetencyAssessments => _competencyAssessments.AsReadOnly();
    /// <summary>Latest observed session odometer value in kilometres, derived from <see cref="OdometerReadings"/> and never used as Fleet's global source of truth.</summary>
    public decimal? LatestOdometerKilometers => _odometerReadings.OrderByDescending(x => x.ObservedAtUtc).Select(x => (decimal?)x.OdometerKilometers).FirstOrDefault();
    /// <summary>First energy level observed for this session, when available.</summary>
    public decimal? StartEnergyLevelPercent => _energyEntries.Where(x => x.EnergyLevelPercent.HasValue).OrderBy(x => x.ObservedAtUtc).Select(x => x.EnergyLevelPercent).FirstOrDefault();
    /// <summary>Latest energy level observed during this session, when available.</summary>
    public decimal? LatestEnergyLevelPercent => _energyEntries.Where(x => x.EnergyLevelPercent.HasValue).OrderByDescending(x => x.ObservedAtUtc).Select(x => x.EnergyLevelPercent).FirstOrDefault();
    /// <summary>Total fuel quantity added during this session context, in litres.</summary>
    public decimal FuelAddedLiters => _energyEntries.Where(x => x.Type == TrainingSessionEnergyEntryType.FuelAdded).Sum(x => x.Quantity ?? 0m);
    /// <summary>Total charging energy recorded during this session context, in kWh.</summary>
    public decimal ChargedEnergyKwh => _energyEntries.Where(x => x.Type == TrainingSessionEnergyEntryType.Charging).Sum(x => x.Quantity ?? 0m);
    /// <summary>Immutable completion report created exactly once when the executed session is closed.</summary>
    public SessionReport? Report => _report;
    /// <summary>Actual UTC end of the delivered session. It is set only when completion succeeds.</summary>
    public DateTimeOffset? ActualEndAtUtc { get; private set; }
    /// <summary>Vehicle energy level observed at the end of the session, expressed as a percentage when provided.</summary>
    public decimal? EndEnergyLevelPercent { get; private set; }
    /// <summary>Gross elapsed minutes between actual start and actual end, before removing interruptions.</summary>
    public int? GrossDurationMinutes { get; private set; }
    /// <summary>Total minutes spent in recorded interruptions during the executed session.</summary>
    public int? InterruptionDurationMinutes { get; private set; }
    /// <summary>Actual delivered teaching minutes after removing recorded interruptions.</summary>
    public int? DeliveredDurationMinutes { get; private set; }
    /// <summary>Distance observed during this session, derived from the first and last odometer readings when both are available.</summary>
    public decimal? DistanceKilometers { get; private set; }
    /// <summary>Operation identifier that makes session completion idempotent across retries and unstable mobile networks.</summary>
    public Guid? CompletionOperationId { get; private set; }
    /// <summary>Fingerprint of the completion request associated with <see cref="CompletionOperationId"/>.</summary>
    public string? CompletionRequestFingerprint { get; private set; }
    /// <summary>Authenticated user who completed and froze the execution report.</summary>
    public UserId? CompletedByUserId { get; private set; }
    /// <summary>UTC instant at which the session became Completed and its execution report became immutable.</summary>
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    /// <summary>Cancellation aggregate created when an already-started execution is definitively stopped; null for normal completion or pre-start Scheduling cancellation.</summary>
    public SessionCancellationId? CancellationId { get; private set; }
    /// <summary>Actual UTC instant at which an already-started session was definitively stopped.</summary>
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    /// <summary>Authenticated user who definitively stopped the already-started session.</summary>
    public UserId? CancelledByUserId { get; private set; }
    /// <summary>UTC timestamp at which the Training Delivery aggregate was materialized.</summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }
    /// <summary>User who materialized the session, when initiated by an authenticated actor.</summary>
    public UserId? CreatedByUserId { get; private set; }
    /// <summary>UTC timestamp of the latest modification.</summary>
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    /// <summary>Authenticated user responsible for the latest modification.</summary>
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<TrainingSession> Materialize(TrainingSessionId id, TrainingSessionMaterialization source, UserId? actor, DateTimeOffset now)
    {
        if (id.IsEmpty) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidIdentifier);
        if (source.OrganizationId.IsEmpty || source.StudentOwnerOrganizationId.IsEmpty || source.PerformingOrganizationId.IsEmpty) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidOrganization);
        if (source.SourceBookingId.IsEmpty) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidBooking);
        if (source.StudentId.IsEmpty) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidStudent);
        if (source.TrainingPathId.IsEmpty) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidTrainingPath);
        if (source.InstructorId.IsEmpty) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidInstructor);
        if (source.PlannedEndAtUtc <= source.PlannedStartAtUtc) return Result.Failure<TrainingSession>(TrainingSessionErrors.InvalidPeriod);

        var session = new TrainingSession(id, source);
        session.CreatedAtUtc = now.ToUniversalTime();
        session.CreatedByUserId = actor;
        session.RaiseDomainEvent(new TrainingSessionScheduledDomainEvent(session.Id, session.OrganizationId, session.SourceBookingId, session.StudentId));
        return Result.Success(session);
    }

    public Result MarkReady(TrainingSessionReadinessSnapshot readiness, UserId actor, DateTimeOffset now, int preparationLeadMinutes)
    {
        if (Status is not (TrainingSessionStatus.Scheduled or TrainingSessionStatus.Ready))
            return Result.Failure(TrainingSessionErrors.InvalidStatusForPreparation);
        if (actor.IsEmpty) return Result.Failure(TrainingSessionErrors.InvalidActor);
        if (!readiness.IsReady) return Result.Failure(TrainingSessionErrors.ResourcesNotReady);

        DateTimeOffset utcNow = now.ToUniversalTime();
        DateTimeOffset currentPlannedStart = readiness.PlannedStartAtUtc.ToUniversalTime();
        if (utcNow < currentPlannedStart.AddMinutes(-Math.Max(0, preparationLeadMinutes)))
            return Result.Failure(TrainingSessionErrors.PreparationTooEarly);

        ReadinessCheckedAtUtc = utcNow;
        ReadinessCheckedByUserId = actor;
        ReadyInstructorId = readiness.InstructorId;
        ReadyVehicleId = readiness.VehicleId;
        ReadyBranchId = readiness.BranchId;
        ReadyPlannedStartAtUtc = currentPlannedStart;
        ReadyPlannedEndAtUtc = readiness.PlannedEndAtUtc.ToUniversalTime();
        Status = TrainingSessionStatus.Ready;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionReadyDomainEvent(Id, OrganizationId, SourceBookingId, readiness.InstructorId, readiness.VehicleId, utcNow));
        return Result.Success();
    }

    public Result Start(
        Guid operationId,
        TrainingSessionReadinessSnapshot readiness,
        UserId actor,
        DateTimeOffset startedAtUtc,
        int startEarlyToleranceMinutes,
        int startLateToleranceMinutes,
        int readinessValidityMinutes)
    {
        DateTimeOffset actualStart = startedAtUtc.ToUniversalTime();
        string fingerprint = BuildOperationFingerprint(
            actualStart.ToString("O"),
            readiness.InstructorId.Value,
            readiness.VehicleId,
            readiness.BranchId?.Value,
            actor.Value);

        if (StartOperationId == operationId)
            return StartRequestFingerprint == fingerprint
                ? Result.Success()
                : Result.Failure(TrainingSessionErrors.StartOperationConflict);

        if (Status != TrainingSessionStatus.Ready)
            return Result.Failure(TrainingSessionErrors.InvalidStatusForStart);
        if (operationId == Guid.Empty || actor.IsEmpty)
            return Result.Failure(TrainingSessionErrors.InvalidActor);
        if (!readiness.IsReady)
            return Result.Failure(TrainingSessionErrors.ResourcesNotReady);

        DateTimeOffset currentPlannedStart = readiness.PlannedStartAtUtc.ToUniversalTime();
        if (actualStart < currentPlannedStart.AddMinutes(-Math.Max(0, startEarlyToleranceMinutes)))
            return Result.Failure(TrainingSessionErrors.StartTooEarly);
        if (actualStart > readiness.PlannedEndAtUtc.ToUniversalTime().AddMinutes(Math.Max(0, startLateToleranceMinutes)))
            return Result.Failure(TrainingSessionErrors.StartTooLate);
        if (!ReadinessCheckedAtUtc.HasValue || actualStart - ReadinessCheckedAtUtc.Value > TimeSpan.FromMinutes(Math.Max(1, readinessValidityMinutes)))
            return Result.Failure(TrainingSessionErrors.ReadinessExpired);

        ActualInstructorId = readiness.InstructorId;
        ActualVehicleId = readiness.VehicleId;
        ActualBranchId = readiness.BranchId;
        ActualStartAtUtc = actualStart;
        StartedByUserId = actor;
        StartOperationId = operationId;
        StartRequestFingerprint = fingerprint;
        Status = TrainingSessionStatus.InProgress;
        LastModifiedAtUtc = actualStart;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionStartedDomainEvent(
            Id, OrganizationId, SourceBookingId, StudentId, readiness.InstructorId, readiness.VehicleId, actualStart));
        return Result.Success();
    }

    // Compatibility overload used by existing domain tests and internal callers that predate explicit offline idempotency.
    public Result Start(
        TrainingSessionReadinessSnapshot readiness,
        UserId actor,
        DateTimeOffset startedAtUtc,
        int startEarlyToleranceMinutes,
        int startLateToleranceMinutes,
        int readinessValidityMinutes) =>
        Start(Guid.NewGuid(), readiness, actor, startedAtUtc, startEarlyToleranceMinutes, startLateToleranceMinutes, readinessValidityMinutes);

    public Result ValidateStartReplay(Guid operationId, DateTimeOffset startedAtUtc, UserId actor)
    {
        if (!StartOperationId.HasValue || StartOperationId.Value != operationId || !ActualStartAtUtc.HasValue || actor.IsEmpty)
            return Result.Failure(TrainingSessionErrors.InvalidStatusForStart);

        string fingerprint = BuildOperationFingerprint(
            startedAtUtc.ToUniversalTime().ToString("O"),
            ActualInstructorId?.Value,
            ActualVehicleId,
            ActualBranchId?.Value,
            actor.Value);

        return StartRequestFingerprint == fingerprint
            ? Result.Success()
            : Result.Failure(TrainingSessionErrors.StartOperationConflict);
    }

    public Result<SessionIntervention> RecordIntervention(
        Guid operationId,
        TrainingSessionInterventionType type,
        TrainingSessionInterventionSeverity severity,
        DateTimeOffset occurredAtUtc,
        string context,
        string reason,
        CompetencyId? relatedCompetencyId,
        string? outcome,
        string? internalComment,
        string? sharedExplanation,
        UserId actor,
        DateTimeOffset now)
    {
        string fingerprint = BuildOperationFingerprint(
            (int)type,
            (int)severity,
            occurredAtUtc.ToUniversalTime().ToString("O"),
            context,
            reason,
            relatedCompetencyId?.Value,
            outcome,
            internalComment,
            sharedExplanation);
        SessionIntervention? existing = _interventions.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
            return existing.RequestFingerprint == fingerprint ? Result.Success(existing) : Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionOperationConflict);
        if (Status != TrainingSessionStatus.InProgress) return Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionRequiresInProgress);
        if (!ActualStartAtUtc.HasValue) return Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionRequiresInProgress);

        DateTimeOffset occurred = occurredAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        if (occurred < ActualStartAtUtc.Value || occurred > utcNow.AddMinutes(5))
            return Result.Failure<SessionIntervention>(TrainingSessionErrors.InterventionOccurredAtInvalid);

        Result<SessionIntervention> created = SessionIntervention.Create(
            TrainingSessionInterventionId.New(),
            Id,
            operationId,
            fingerprint,
            type,
            severity,
            occurred,
            context,
            reason,
            relatedCompetencyId,
            outcome,
            internalComment,
            sharedExplanation,
            actor,
            utcNow);
        if (created.IsFailure) return created;

        _interventions.Add(created.Value);
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionInterventionRecordedDomainEvent(Id, OrganizationId, created.Value.Id, type, severity, occurred));
        return created;
    }

    public Result<SessionObservation> RecordObservation(
        Guid operationId,
        TrainingSessionObservationType type,
        DateTimeOffset observedAtUtc,
        string content,
        bool isInternal,
        UserId actor,
        DateTimeOffset now)
    {
        string fingerprint = BuildOperationFingerprint((int)type, observedAtUtc.ToUniversalTime().ToString("O"), content, isInternal);
        SessionObservation? existing = _observations.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
            return existing.RequestFingerprint == fingerprint ? Result.Success(existing) : Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationOperationConflict);
        if (Status is not (TrainingSessionStatus.InProgress or TrainingSessionStatus.Interrupted))
            return Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationInvalidSessionStatus);
        if (!ActualStartAtUtc.HasValue) return Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationInvalidSessionStatus);

        DateTimeOffset observed = observedAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        if (observed < ActualStartAtUtc.Value || observed > utcNow.AddMinutes(5))
            return Result.Failure<SessionObservation>(TrainingSessionErrors.ObservationObservedAtInvalid);

        Result<SessionObservation> created = SessionObservation.Create(
            TrainingSessionObservationId.New(), Id, operationId, fingerprint, type, observed, content, isInternal, actor, utcNow);
        if (created.IsFailure) return created;

        _observations.Add(created.Value);
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionObservationRecordedDomainEvent(Id, OrganizationId, created.Value.Id, type, observed));
        return created;
    }

    public Result<SessionMarker> RecordMarker(
        Guid operationId,
        TrainingSessionMarkerType type,
        DateTimeOffset occurredAtUtc,
        CompetencyId? competencyId,
        string shortNote,
        TrainingSessionMarkerSeverity severity,
        decimal? latitude,
        decimal? longitude,
        bool createdOffline,
        UserId actor,
        DateTimeOffset now)
    {
        string fingerprint = BuildOperationFingerprint((int)type, occurredAtUtc.ToUniversalTime().ToString("O"), competencyId?.Value, shortNote, (int)severity, latitude, longitude, createdOffline);
        SessionMarker? existing = _markers.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
            return existing.RequestFingerprint == fingerprint ? Result.Success(existing) : Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerOperationConflict);
        if (Status is not (TrainingSessionStatus.InProgress or TrainingSessionStatus.Interrupted))
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerInvalidSessionStatus);
        if (!ActualStartAtUtc.HasValue) return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerInvalidSessionStatus);

        DateTimeOffset occurred = occurredAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        if (occurred < ActualStartAtUtc.Value || occurred > utcNow.AddMinutes(5))
            return Result.Failure<SessionMarker>(TrainingSessionErrors.MarkerOccurredAtInvalid);

        Result<SessionMarker> created = SessionMarker.Create(
            TrainingSessionMarkerId.New(), Id, operationId, fingerprint, type, occurred, competencyId, shortNote, severity, latitude, longitude, createdOffline, actor, utcNow);
        if (created.IsFailure) return created;

        _markers.Add(created.Value);
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionMarkerRecordedDomainEvent(Id, OrganizationId, created.Value.Id, type, occurred));
        return created;
    }

    public Result<SessionInterruption> Interrupt(
        Guid operationId,
        TrainingSessionInterruptionReason reason,
        string? description,
        DateTimeOffset interruptedAtUtc,
        UserId actor,
        DateTimeOffset now)
    {
        DateTimeOffset utcNow = now.ToUniversalTime();
        DateTimeOffset interruptedAt = interruptedAtUtc.ToUniversalTime();
        string fingerprint = BuildOperationFingerprint((int)reason, description, interruptedAt.ToString("O"));
        SessionInterruption? existing = _interruptions.FirstOrDefault(x => x.InterruptOperationId == operationId);
        if (existing is not null)
            return existing.InterruptRequestFingerprint == fingerprint ? Result.Success(existing) : Result.Failure<SessionInterruption>(TrainingSessionErrors.InterruptionOperationConflict);
        if (Status != TrainingSessionStatus.InProgress) return Result.Failure<SessionInterruption>(TrainingSessionErrors.InterruptionInvalidSessionStatus);
        if (_interruptions.Any(x => x.IsActive)) return Result.Failure<SessionInterruption>(TrainingSessionErrors.InterruptionAlreadyActive);
        if (!ActualStartAtUtc.HasValue || interruptedAt < ActualStartAtUtc.Value || interruptedAt > utcNow.AddMinutes(5))
            return Result.Failure<SessionInterruption>(TrainingSessionErrors.InterruptionOccurredAtInvalid);

        Result<SessionInterruption> created = SessionInterruption.Create(
            TrainingSessionInterruptionId.New(), Id, operationId, fingerprint, reason, description, interruptedAt, actor);
        if (created.IsFailure) return created;

        _interruptions.Add(created.Value);
        Status = TrainingSessionStatus.Interrupted;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionInterruptedDomainEvent(Id, OrganizationId, created.Value.Id, reason, interruptedAt));
        return created;
    }

    public Result Resume(Guid operationId, DateTimeOffset resumedAtUtc, UserId actor, DateTimeOffset now)
    {
        DateTimeOffset resumedAt = resumedAtUtc.ToUniversalTime();
        SessionInterruption? existingResume = _interruptions.FirstOrDefault(x => x.ResumeOperationId == operationId);
        if (existingResume is not null)
        {
            string retryFingerprint = BuildOperationFingerprint(existingResume.Id.Value, resumedAt.ToString("O"));
            return existingResume.ResumeRequestFingerprint == retryFingerprint
                ? Result.Success()
                : Result.Failure(TrainingSessionErrors.InterruptionOperationConflict);
        }
        if (Status != TrainingSessionStatus.Interrupted) return Result.Failure(TrainingSessionErrors.ResumeInvalidSessionStatus);
        SessionInterruption? interruption = _interruptions.LastOrDefault(x => x.IsActive);
        if (interruption is null) return Result.Failure(TrainingSessionErrors.InterruptionNotActive);

        DateTimeOffset utcNow = now.ToUniversalTime();
        if (resumedAt > utcNow.AddMinutes(5)) return Result.Failure(TrainingSessionErrors.InterruptionResumeAtInvalid);
        string fingerprint = BuildOperationFingerprint(interruption.Id.Value, resumedAt.ToString("O"));
        Result resumed = interruption.Resume(operationId, fingerprint, resumedAt, actor);
        if (resumed.IsFailure) return resumed;

        Status = TrainingSessionStatus.InProgress;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionResumedDomainEvent(Id, OrganizationId, interruption.Id, resumedAt));
        return Result.Success();
    }

    public Result<SessionOdometerReading> RecordOdometer(
        Guid operationId,
        decimal odometerKilometers,
        TrainingSessionOdometerSource source,
        DateTimeOffset observedAtUtc,
        UserId actor,
        DateTimeOffset now)
    {
        string fingerprint = BuildOperationFingerprint(odometerKilometers, (int)source, observedAtUtc.ToUniversalTime().ToString("O"));
        SessionOdometerReading? existing = _odometerReadings.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
            return existing.RequestFingerprint == fingerprint ? Result.Success(existing) : Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerOperationConflict);
        Guid? currentVehicleId = ActualVehicleId ?? ReadyVehicleId;
        if (!currentVehicleId.HasValue) return Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerVehicleRequired);
        if (Status is not (TrainingSessionStatus.Ready or TrainingSessionStatus.InProgress or TrainingSessionStatus.Interrupted))
            return Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerInvalidSessionStatus);

        DateTimeOffset observed = observedAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        if (observed > utcNow.AddMinutes(5)) return Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerObservedAtInvalid);
        SessionOdometerReading? latest = _odometerReadings.OrderByDescending(x => x.ObservedAtUtc).FirstOrDefault();
        if (latest is not null && observed < latest.ObservedAtUtc)
            return Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerObservedAtOutOfOrder);
        if (latest is not null && odometerKilometers < latest.OdometerKilometers)
            return Result.Failure<SessionOdometerReading>(TrainingSessionErrors.OdometerMustBeMonotonic);

        Result<SessionOdometerReading> created = SessionOdometerReading.Create(
            TrainingSessionOdometerReadingId.New(), Id, operationId, fingerprint, odometerKilometers, source, observed, actor, utcNow);
        if (created.IsFailure) return created;

        _odometerReadings.Add(created.Value);
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionOdometerRecordedDomainEvent(Id, OrganizationId, currentVehicleId.Value, created.Value.Id, created.Value.OdometerKilometers, created.Value.ObservedAtUtc));
        return created;
    }

    public Result<SessionEnergyEntry> RecordEnergy(
        Guid operationId,
        TrainingSessionEnergyEntryType type,
        decimal? energyLevelPercent,
        decimal? quantity,
        DateTimeOffset observedAtUtc,
        string? note,
        bool createdOffline,
        UserId actor,
        DateTimeOffset now)
    {
        string fingerprint = BuildOperationFingerprint((int)type, energyLevelPercent, quantity, observedAtUtc.ToUniversalTime().ToString("O"), note?.Trim(), createdOffline);
        SessionEnergyEntry? existing = _energyEntries.FirstOrDefault(x => x.OperationId == operationId);
        if (existing is not null)
            return existing.RequestFingerprint == fingerprint ? Result.Success(existing) : Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyOperationConflict);

        Guid? currentVehicleId = ActualVehicleId ?? ReadyVehicleId;
        if (!currentVehicleId.HasValue) return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyVehicleRequired);
        if (Status is not (TrainingSessionStatus.Ready or TrainingSessionStatus.InProgress or TrainingSessionStatus.Interrupted))
            return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyInvalidSessionStatus);

        DateTimeOffset observed = observedAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        if (observed > utcNow.AddMinutes(5)) return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyObservedAtInvalid);
        SessionEnergyEntry? latest = _energyEntries.OrderByDescending(x => x.ObservedAtUtc).FirstOrDefault();
        if (latest is not null && observed < latest.ObservedAtUtc) return Result.Failure<SessionEnergyEntry>(TrainingSessionErrors.EnergyObservedAtOutOfOrder);

        Result<SessionEnergyEntry> created = SessionEnergyEntry.Create(TrainingSessionEnergyEntryId.New(), Id, operationId, fingerprint, type, energyLevelPercent, quantity, observed, note, createdOffline, actor, utcNow);
        if (created.IsFailure) return created;

        _energyEntries.Add(created.Value);
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionEnergyRecordedDomainEvent(Id, OrganizationId, currentVehicleId.Value, created.Value.Id, created.Value.Type, created.Value.EnergyLevelPercent, created.Value.Quantity, created.Value.ObservedAtUtc));
        return created;
    }

    public Result<SessionAttendance> RecordAttendance(
        Guid operationId,
        TrainingSessionAttendanceStatus status,
        DateTimeOffset? actualArrivalAtUtc,
        DateTimeOffset? actualDepartureAtUtc,
        string? reason,
        Guid? evidenceDocumentId,
        UserId actor,
        DateTimeOffset now,
        int earlyToleranceMinutes)
    {
        string requestFingerprint = BuildAttendanceFingerprint(status, actualArrivalAtUtc, actualDepartureAtUtc, reason, evidenceDocumentId, false, null);
        SessionAttendance? existingOperation = _attendanceHistory.FirstOrDefault(x => x.OperationId == operationId);
        if (existingOperation is not null)
            return existingOperation.RequestFingerprint == requestFingerprint
                ? Result.Success(existingOperation)
                : Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceOperationConflict);
        if (CurrentAttendanceId.HasValue) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceAlreadyRecorded);
        if (actor.IsEmpty || operationId == Guid.Empty) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalid);
        if (Status is TrainingSessionStatus.Completed or TrainingSessionStatus.Cancelled) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalidSessionStatus);

        DateTimeOffset utcNow = now.ToUniversalTime();
        DateTimeOffset plannedStart = (ReadyPlannedStartAtUtc ?? PlannedStartAtUtc).ToUniversalTime();
        if (utcNow < plannedStart.AddMinutes(-Math.Max(0, earlyToleranceMinutes)))
            return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceTooEarly);

        Result normalized = NormalizeAttendanceTimes(status, ref actualArrivalAtUtc, ref actualDepartureAtUtc, utcNow, plannedStart, true);
        if (normalized.IsFailure) return Result.Failure<SessionAttendance>(normalized.Error);

        int lateMinutes = CalculateLateMinutes(status, actualArrivalAtUtc, plannedStart);
        Result<SessionAttendance> created = SessionAttendance.Create(
            TrainingSessionAttendanceId.New(), Id, operationId, requestFingerprint, 1, status, actualArrivalAtUtc, actualDepartureAtUtc, lateMinutes,
            reason, evidenceDocumentId, actor, utcNow, null, false, null);
        if (created.IsFailure) return created;

        _attendanceHistory.Add(created.Value);
        CurrentAttendanceId = created.Value.Id;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseAttendanceEvents(created.Value, actor, false);
        return created;
    }

    public Result<SessionAttendance> CorrectAttendance(
        Guid operationId,
        TrainingSessionAttendanceStatus status,
        DateTimeOffset? actualArrivalAtUtc,
        DateTimeOffset? actualDepartureAtUtc,
        string? reason,
        Guid? evidenceDocumentId,
        UserId actor,
        DateTimeOffset now,
        int correctionWindowHours,
        bool isOverride,
        string? overrideReason)
    {
        string requestFingerprint = BuildAttendanceFingerprint(status, actualArrivalAtUtc, actualDepartureAtUtc, reason, evidenceDocumentId, isOverride, overrideReason);
        SessionAttendance? existingOperation = _attendanceHistory.FirstOrDefault(x => x.OperationId == operationId);
        if (existingOperation is not null)
            return existingOperation.RequestFingerprint == requestFingerprint
                ? Result.Success(existingOperation)
                : Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceOperationConflict);
        if (actor.IsEmpty || operationId == Guid.Empty) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceInvalid);

        SessionAttendance? current = CurrentAttendanceId.HasValue
            ? _attendanceHistory.FirstOrDefault(x => x.Id == CurrentAttendanceId.Value)
            : null;
        if (current is null) return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceNotRecorded);

        DateTimeOffset utcNow = now.ToUniversalTime();
        if (!isOverride && utcNow - current.RecordedAtUtc > TimeSpan.FromHours(Math.Max(1, correctionWindowHours)))
            return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceCorrectionWindowExpired);
        if (isOverride && string.IsNullOrWhiteSpace(overrideReason))
            return Result.Failure<SessionAttendance>(TrainingSessionErrors.AttendanceOverrideReasonRequired);

        DateTimeOffset plannedStart = (ReadyPlannedStartAtUtc ?? PlannedStartAtUtc).ToUniversalTime();
        Result normalized = NormalizeAttendanceTimes(status, ref actualArrivalAtUtc, ref actualDepartureAtUtc, utcNow, plannedStart, false);
        if (normalized.IsFailure) return Result.Failure<SessionAttendance>(normalized.Error);

        Result<SessionAttendance> created = SessionAttendance.Create(
            TrainingSessionAttendanceId.New(), Id, operationId, requestFingerprint, current.Revision + 1, status, actualArrivalAtUtc, actualDepartureAtUtc,
            CalculateLateMinutes(status, actualArrivalAtUtc, plannedStart), reason, evidenceDocumentId, actor, utcNow, current.Id, isOverride, overrideReason);
        if (created.IsFailure) return created;

        _attendanceHistory.Add(created.Value);
        CurrentAttendanceId = created.Value.Id;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new TrainingSessionAttendanceCorrectedDomainEvent(Id, OrganizationId, current.Id, created.Value.Id, actor, utcNow, isOverride));
        RaiseAttendanceEvents(created.Value, actor, true);
        return created;
    }

    public Result FinishExecution(
        Guid operationId,
        DateTimeOffset actualEndAtUtc,
        decimal? endEnergyLevelPercent,
        UserId actor,
        DateTimeOffset now)
    {
        DateTimeOffset actualEnd = actualEndAtUtc.ToUniversalTime();
        string fingerprint = BuildOperationFingerprint(actualEnd.ToString("O"), endEnergyLevelPercent?.ToString(System.Globalization.CultureInfo.InvariantCulture));

        if (CompletionOperationId == operationId)
            return CompletionRequestFingerprint == fingerprint ? Result.Success() : Result.Failure(TrainingSessionErrors.CompletionOperationConflict);
        if (Status == TrainingSessionStatus.Completed)
            return Result.Failure(TrainingSessionErrors.CompletionAlreadyCompleted);
        if (_interruptions.Any(x => x.IsActive))
            return Result.Failure(TrainingSessionErrors.CompletionActiveInterruption);
        if (Status != TrainingSessionStatus.InProgress || !ActualStartAtUtc.HasValue)
            return Result.Failure(TrainingSessionErrors.CompletionRequiresInProgress);
        if (actor.IsEmpty || operationId == Guid.Empty)
            return Result.Failure(TrainingSessionErrors.CompletionInvalid);
        if (endEnergyLevelPercent is < 0 or > 100)
            return Result.Failure(TrainingSessionErrors.CompletionEnergyLevelInvalid);

        bool hasAnyCreditReference = TrainingCreditAccountId.HasValue || CreditQuantity.HasValue || !string.IsNullOrWhiteSpace(CreditReservationReference);
        bool hasCompleteCreditReference = TrainingCreditAccountId.HasValue && CreditQuantity is > 0 && !string.IsNullOrWhiteSpace(CreditReservationReference);
        if (hasAnyCreditReference && !hasCompleteCreditReference)
            return Result.Failure(TrainingSessionErrors.CompletionCreditReservationInvalid);

        DateTimeOffset utcNow = now.ToUniversalTime();
        if (actualEnd <= ActualStartAtUtc.Value || actualEnd > utcNow.AddMinutes(5))
            return Result.Failure(TrainingSessionErrors.CompletionActualEndInvalid);

        SessionAttendance? attendance = CurrentAttendanceId.HasValue ? _attendanceHistory.FirstOrDefault(x => x.Id == CurrentAttendanceId.Value) : null;
        if (attendance is null) return Result.Failure(TrainingSessionErrors.CompletionAttendanceRequired);
        if (attendance.Status is TrainingSessionAttendanceStatus.StudentAbsent or TrainingSessionAttendanceStatus.InstructorAbsent or TrainingSessionAttendanceStatus.ExcusedAbsence or TrainingSessionAttendanceStatus.UnexcusedAbsence or TrainingSessionAttendanceStatus.UnableToDeliver)
            return Result.Failure(TrainingSessionErrors.CompletionAttendanceIncompatible);

        Result finalizeAttendance = FinalizeAttendanceAtCompletion(attendance, operationId, actualEnd, actor, utcNow);
        if (finalizeAttendance.IsFailure) return finalizeAttendance;

        int grossMinutes = Math.Max(1, RoundMinutes(actualEnd - ActualStartAtUtc.Value));
        int interruptionMinutes = _interruptions.Where(x => x.ResumedAtUtc.HasValue).Sum(x => Math.Max(0, RoundMinutes(x.ResumedAtUtc!.Value - x.StartedAtUtc)));
        int deliveredMinutes = grossMinutes - interruptionMinutes;
        if (deliveredMinutes <= 0) return Result.Failure(TrainingSessionErrors.CompletionDurationInvalid);

        decimal? distance = CalculateSessionDistance();
        ActualEndAtUtc = actualEnd;
        EndEnergyLevelPercent = endEnergyLevelPercent;
        GrossDurationMinutes = grossMinutes;
        InterruptionDurationMinutes = interruptionMinutes;
        DeliveredDurationMinutes = deliveredMinutes;
        DistanceKilometers = distance;
        CompletionOperationId = operationId;
        CompletionRequestFingerprint = fingerprint;
        CompletedByUserId = actor;
        CompletedAtUtc = utcNow;
        Status = TrainingSessionStatus.Completed;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;

        UserId actualInstructor = ActualInstructorId ?? ReadyInstructorId ?? InstructorId;
        RaiseDomainEvent(new TrainingSessionCompletedDomainEvent(Id, OrganizationId, StudentOwnerOrganizationId, PerformingOrganizationId, SourceBookingId, StudentId, TrainingPathId, actualInstructor, ActualVehicleId ?? ReadyVehicleId ?? VehicleId, ActualStartAtUtc.Value, actualEnd, deliveredMinutes, distance));
        RaiseDomainEvent(new TrainingSessionBillableDomainEvent(Id, OrganizationId, StudentOwnerOrganizationId, PerformingOrganizationId, StudentId, PricingReference, deliveredMinutes, utcNow));
        if (TrainingCreditAccountId.HasValue && CreditQuantity is > 0 && !string.IsNullOrWhiteSpace(CreditReservationReference))
            RaiseDomainEvent(new TrainingCreditConsumptionRequestedDomainEvent(Id, OrganizationId, StudentId, TrainingCreditAccountId.Value, CreditQuantity.Value, CreditReservationReference!, utcNow));

        return Result.Success();
    }

    public Result<SessionReport> SaveReportDraft(
        Guid operationId,
        int expectedVersion,
        int lastCompletedStep,
        string? summary,
        string? objectivesWorked,
        string? objectivesAchieved,
        string? nextObjective,
        string? sharedComment,
        string? internalNote,
        UserId actor,
        DateTimeOffset now)
    {
        if (Status != TrainingSessionStatus.Completed || !ActualEndAtUtc.HasValue || !GrossDurationMinutes.HasValue || !InterruptionDurationMinutes.HasValue || !DeliveredDurationMinutes.HasValue)
            return Result.Failure<SessionReport>(TrainingSessionErrors.ReportDraftRequiresCompletedSession);

        if (_report is null)
        {
            if (expectedVersion != 0)
                return Result.Failure<SessionReport>(TrainingSessionErrors.ReportDraftVersionConflict);

            Result<SessionReport> created = SessionReport.CreateDraft(
                TrainingSessionReportId.New(),
                Id,
                operationId,
                ActualEndAtUtc.Value,
                GrossDurationMinutes.Value,
                InterruptionDurationMinutes.Value,
                DeliveredDurationMinutes.Value,
                DistanceKilometers,
                lastCompletedStep,
                summary,
                objectivesWorked,
                objectivesAchieved,
                nextObjective,
                sharedComment,
                internalNote,
                actor,
                now);
            if (created.IsFailure) return created;
            _report = created.Value;
        }
        else
        {
            Result saved = _report.SaveDraft(
                operationId,
                expectedVersion,
                lastCompletedStep,
                summary,
                objectivesWorked,
                objectivesAchieved,
                nextObjective,
                sharedComment,
                internalNote,
                actor,
                now);
            if (saved.IsFailure) return Result.Failure<SessionReport>(saved.Error);
        }

        LastModifiedAtUtc = now.ToUniversalTime();
        LastModifiedByUserId = actor;
        return Result.Success(_report);
    }

    public Result MarkReportReadyToSubmit(Guid operationId, int expectedVersion, UserId actor, DateTimeOffset now)
    {
        if (_report is null) return Result.Failure(TrainingSessionErrors.ReportDraftRequiresCompletedSession);
        if (CurrentAttendanceId is null || DeliveredDurationMinutes is not > 0 || _competencyAssessments.Count == 0 ||
            string.IsNullOrWhiteSpace(_report.Summary) || string.IsNullOrWhiteSpace(_report.NextObjective) ||
            _markers.Any(m => string.IsNullOrWhiteSpace(m.ShortNote)) ||
            ((ActualVehicleId ?? ReadyVehicleId ?? VehicleId).HasValue && !DistanceKilometers.HasValue))
            return Result.Failure(TrainingSessionErrors.ReportNotReadyToSubmit);
        Result result = _report.MarkReadyToSubmit(operationId, expectedVersion, actor, now);
        if (result.IsSuccess) { LastModifiedAtUtc = now.ToUniversalTime(); LastModifiedByUserId = actor; }
        return result;
    }

    public Result SubmitReport(Guid operationId, int expectedVersion, bool requestSupervisorReview, UserId actor, DateTimeOffset now)
    {
        if (_report is null) return Result.Failure(TrainingSessionErrors.ReportDraftRequiresCompletedSession);
        Result result = _report.Submit(operationId, expectedVersion, requestSupervisorReview, actor, now);
        if (result.IsSuccess) { LastModifiedAtUtc = now.ToUniversalTime(); LastModifiedByUserId = actor; }
        return result;
    }

    public Result UpdateSharedComment(Guid operationId, int expectedVersion, string? content, UserId actor, DateTimeOffset now)
    {
        if (_report is null) return Result.Failure(TrainingSessionErrors.ReportDraftRequiresCompletedSession);
        Result result = _report.UpdateNarrative(operationId, expectedVersion, SessionReportNarrativeKind.SharedComment, content, actor, now);
        if (result.IsSuccess) { LastModifiedAtUtc = now.ToUniversalTime(); LastModifiedByUserId = actor; }
        return result;
    }

    public Result UpdateInternalNote(Guid operationId, int expectedVersion, string? content, UserId actor, DateTimeOffset now)
    {
        if (_report is null) return Result.Failure(TrainingSessionErrors.ReportDraftRequiresCompletedSession);
        Result result = _report.UpdateNarrative(operationId, expectedVersion, SessionReportNarrativeKind.InternalNote, content, actor, now);
        if (result.IsSuccess) { LastModifiedAtUtc = now.ToUniversalTime(); LastModifiedByUserId = actor; }
        return result;
    }

    public Result<SessionReportRevision> RequestReportRevision(Guid operationId, int expectedVersion, SessionReportRevisionScenario scenario, string fieldCode, string currentValue, string proposedValue, string reason, bool hasFinancialImpact, bool approvalRequired, UserId actor, DateTimeOffset now)
    {
        if (_report is null) return Result.Failure<SessionReportRevision>(TrainingSessionErrors.ReportDraftRequiresCompletedSession);
        Result<SessionReportRevision> result = _report.RequestRevision(TrainingSessionReportRevisionId.New(), operationId, expectedVersion, scenario, fieldCode, currentValue, proposedValue, reason, hasFinancialImpact, approvalRequired, actor, now);
        if (result.IsSuccess) { LastModifiedAtUtc = now.ToUniversalTime(); LastModifiedByUserId = actor; }
        return result;
    }

    public Result DecideReportRevision(TrainingSessionReportRevisionId revisionId, bool approve, string? decisionReason, UserId actor, DateTimeOffset now)
    {
        if (_report is null) return Result.Failure(TrainingSessionErrors.ReportDraftRequiresCompletedSession);
        Result result = _report.DecideRevision(revisionId, approve, decisionReason, actor, now);
        if (result.IsSuccess) { LastModifiedAtUtc = now.ToUniversalTime(); LastModifiedByUserId = actor; }
        return result;
    }

    public Result<SessionReport> Complete(
        Guid operationId,
        DateTimeOffset actualEndAtUtc,
        string summary,
        string? objectivesWorked,
        string? objectivesAchieved,
        string? nextObjective,
        string? instructorComments,
        UserId actor,
        DateTimeOffset now)
    {
        DateTimeOffset actualEnd = actualEndAtUtc.ToUniversalTime();
        string fingerprint = BuildOperationFingerprint(
            actualEnd.ToString("O"), summary, objectivesWorked, objectivesAchieved, nextObjective, instructorComments);

        if (CompletionOperationId == operationId)
            return CompletionRequestFingerprint == fingerprint && _report is not null
                ? Result.Success(_report!)
                : Result.Failure<SessionReport>(TrainingSessionErrors.CompletionOperationConflict);

        if (Status == TrainingSessionStatus.Completed)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionAlreadyCompleted);
        if (_interruptions.Any(x => x.IsActive))
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionActiveInterruption);
        if (Status != TrainingSessionStatus.InProgress)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionRequiresInProgress);
        if (actor.IsEmpty || operationId == Guid.Empty)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionInvalid);
        if (!ActualStartAtUtc.HasValue)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionRequiresInProgress);

        bool hasAnyCreditReference = TrainingCreditAccountId.HasValue || CreditQuantity.HasValue || !string.IsNullOrWhiteSpace(CreditReservationReference);
        bool hasCompleteCreditReference = TrainingCreditAccountId.HasValue && CreditQuantity is > 0 && !string.IsNullOrWhiteSpace(CreditReservationReference);
        if (hasAnyCreditReference && !hasCompleteCreditReference)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionCreditReservationInvalid);

        DateTimeOffset utcNow = now.ToUniversalTime();
        if (actualEnd <= ActualStartAtUtc.Value || actualEnd > utcNow.AddMinutes(5))
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionActualEndInvalid);

        SessionAttendance? attendance = CurrentAttendanceId.HasValue
            ? _attendanceHistory.FirstOrDefault(x => x.Id == CurrentAttendanceId.Value)
            : null;
        if (attendance is null)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionAttendanceRequired);
        if (attendance.Status is TrainingSessionAttendanceStatus.StudentAbsent
            or TrainingSessionAttendanceStatus.InstructorAbsent
            or TrainingSessionAttendanceStatus.ExcusedAbsence
            or TrainingSessionAttendanceStatus.UnexcusedAbsence
            or TrainingSessionAttendanceStatus.UnableToDeliver)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionAttendanceIncompatible);

        Result finalizeAttendance = FinalizeAttendanceAtCompletion(attendance, operationId, actualEnd, actor, utcNow);
        if (finalizeAttendance.IsFailure)
            return Result.Failure<SessionReport>(finalizeAttendance.Error);

        int grossMinutes = Math.Max(1, RoundMinutes(actualEnd - ActualStartAtUtc.Value));
        int interruptionMinutes = _interruptions
            .Where(x => x.ResumedAtUtc.HasValue)
            .Sum(x => Math.Max(0, RoundMinutes(x.ResumedAtUtc!.Value - x.StartedAtUtc)));
        int deliveredMinutes = grossMinutes - interruptionMinutes;
        if (deliveredMinutes <= 0)
            return Result.Failure<SessionReport>(TrainingSessionErrors.CompletionDurationInvalid);

        decimal? distance = CalculateSessionDistance();
        Result<SessionReport> created = SessionReport.Create(
            TrainingSessionReportId.New(),
            Id,
            operationId,
            fingerprint,
            actualEnd,
            grossMinutes,
            interruptionMinutes,
            deliveredMinutes,
            distance,
            summary,
            objectivesWorked,
            objectivesAchieved,
            nextObjective,
            instructorComments,
            actor,
            utcNow);
        if (created.IsFailure) return created;

        _report = created.Value;
        ActualEndAtUtc = actualEnd;
        GrossDurationMinutes = grossMinutes;
        InterruptionDurationMinutes = interruptionMinutes;
        DeliveredDurationMinutes = deliveredMinutes;
        DistanceKilometers = distance;
        CompletionOperationId = operationId;
        CompletionRequestFingerprint = fingerprint;
        CompletedByUserId = actor;
        CompletedAtUtc = utcNow;
        Status = TrainingSessionStatus.Completed;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;

        UserId actualInstructor = ActualInstructorId ?? ReadyInstructorId ?? InstructorId;
        RaiseDomainEvent(new TrainingSessionCompletedDomainEvent(
            Id, OrganizationId, StudentOwnerOrganizationId, PerformingOrganizationId, SourceBookingId,
            StudentId, TrainingPathId, actualInstructor, ActualVehicleId ?? ReadyVehicleId ?? VehicleId,
            ActualStartAtUtc.Value, actualEnd, deliveredMinutes, distance));
        RaiseDomainEvent(new TrainingSessionBillableDomainEvent(
            Id, OrganizationId, StudentOwnerOrganizationId, PerformingOrganizationId, StudentId,
            PricingReference, deliveredMinutes, utcNow));

        if (TrainingCreditAccountId.HasValue
            && CreditQuantity is > 0
            && !string.IsNullOrWhiteSpace(CreditReservationReference))
        {
            RaiseDomainEvent(new TrainingCreditConsumptionRequestedDomainEvent(
                Id, OrganizationId, StudentId, TrainingCreditAccountId.Value, CreditQuantity.Value,
                CreditReservationReference!, utcNow));
        }

        return Result.Success(created.Value);
    }



    public Result<TrainingSessionCancellationFacts> CancelDuringExecution(
        SessionCancellationId cancellationId,
        DateTimeOffset cancelledAtUtc,
        UserId actor,
        DateTimeOffset now)
    {
        if (cancellationId.IsEmpty || actor.IsEmpty)
            return Result.Failure<TrainingSessionCancellationFacts>(SessionCancellationErrors.Invalid);
        if (Status is TrainingSessionStatus.Scheduled or TrainingSessionStatus.Ready)
            return Result.Failure<TrainingSessionCancellationFacts>(SessionCancellationErrors.UseSchedulingBeforeStart);
        if (Status == TrainingSessionStatus.Completed)
            return Result.Failure<TrainingSessionCancellationFacts>(SessionCancellationErrors.SessionAlreadyCompleted);
        if (Status == TrainingSessionStatus.Cancelled)
            return Result.Failure<TrainingSessionCancellationFacts>(SessionCancellationErrors.AlreadyCancelled);
        if (Status is not (TrainingSessionStatus.InProgress or TrainingSessionStatus.Interrupted) || !ActualStartAtUtc.HasValue)
            return Result.Failure<TrainingSessionCancellationFacts>(SessionCancellationErrors.RequiresStartedSession);

        DateTimeOffset cancelledAt = cancelledAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        if (cancelledAt <= ActualStartAtUtc.Value || cancelledAt > utcNow.AddMinutes(5))
            return Result.Failure<TrainingSessionCancellationFacts>(SessionCancellationErrors.CancelledAtInvalid);

        SessionAttendance? attendance = CurrentAttendanceId.HasValue
            ? _attendanceHistory.FirstOrDefault(x => x.Id == CurrentAttendanceId.Value)
            : null;
        if (attendance is not null && attendance.Status is (TrainingSessionAttendanceStatus.Present or TrainingSessionAttendanceStatus.LateArrival) && !attendance.ActualDepartureAtUtc.HasValue)
        {
            Result finalized = FinalizeAttendanceAtTermination(attendance, cancellationId, cancelledAt, actor, utcNow);
            if (finalized.IsFailure) return Result.Failure<TrainingSessionCancellationFacts>(finalized.Error);
        }

        foreach (SessionInterruption interruption in _interruptions.Where(x => x.IsActive))
        {
            Result terminated = interruption.Terminate(cancellationId, cancelledAt);
            if (terminated.IsFailure) return Result.Failure<TrainingSessionCancellationFacts>(terminated.Error);
        }

        int grossMinutes = Math.Max(1, RoundMinutes(cancelledAt - ActualStartAtUtc.Value));
        int interruptionMinutes = _interruptions.Sum(x =>
        {
            DateTimeOffset end = x.ResumedAtUtc ?? x.TerminatedAtUtc ?? cancelledAt;
            return Math.Max(0, RoundMinutes(end - x.StartedAtUtc));
        });
        int deliveredMinutes = Math.Max(0, grossMinutes - interruptionMinutes);
        decimal? distance = CalculateSessionDistance();

        ActualEndAtUtc = cancelledAt;
        GrossDurationMinutes = grossMinutes;
        InterruptionDurationMinutes = interruptionMinutes;
        DeliveredDurationMinutes = deliveredMinutes;
        DistanceKilometers = distance;
        CancellationId = cancellationId;
        CancelledAtUtc = cancelledAt;
        CancelledByUserId = actor;
        Status = TrainingSessionStatus.Cancelled;
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;

        return Result.Success(new TrainingSessionCancellationFacts(
            ActualStartAtUtc.Value, cancelledAt, grossMinutes, interruptionMinutes, deliveredMinutes, distance,
            ActualInstructorId ?? ReadyInstructorId ?? InstructorId, ActualVehicleId ?? ReadyVehicleId ?? VehicleId,
            ActualBranchId ?? ReadyBranchId ?? BranchId));
    }

    public Result<SessionCompetencyAssessment> RecordCompetencyAssessment(
        Guid operationId,
        CompetencyId competencyId,
        CurriculumVersionId curriculumVersionId,
        Guid pedagogyAssessmentId,
        string levelCode,
        string? observedCriteria,
        string? context,
        TrainingSessionInterventionId? relatedInterventionId,
        string? internalComment,
        string? sharedComment,
        Guid? evidenceDocumentId,
        DateTimeOffset assessedAtUtc,
        UserId actor,
        DateTimeOffset now)
    {
        string fingerprint = BuildOperationFingerprint(
            competencyId.Value, curriculumVersionId.Value, levelCode, observedCriteria, context,
            relatedInterventionId?.Value, internalComment, sharedComment, evidenceDocumentId,
            assessedAtUtc.ToUniversalTime().ToString("O"));

        SessionCompetencyAssessment? byOperation = _competencyAssessments.FirstOrDefault(x => x.OperationId == operationId);
        if (byOperation is not null)
            return byOperation.RequestFingerprint == fingerprint
                ? Result.Success(byOperation)
                : Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentOperationConflict);

        if (Status is not (TrainingSessionStatus.InProgress or TrainingSessionStatus.Completed))
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentInvalidSessionStatus);
        if (actor.IsEmpty || competencyId.IsEmpty || curriculumVersionId.IsEmpty || pedagogyAssessmentId == Guid.Empty)
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentInvalid);
        if (_competencyAssessments.Any(x => x.CompetencyId == competencyId))
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentCompetencyAlreadyRecorded);

        DateTimeOffset assessed = assessedAtUtc.ToUniversalTime();
        DateTimeOffset utcNow = now.ToUniversalTime();
        DateTimeOffset earliest = ActualStartAtUtc ?? ReadyPlannedStartAtUtc ?? PlannedStartAtUtc;
        DateTimeOffset latest = ActualEndAtUtc ?? utcNow;
        if (assessed < earliest.AddMinutes(-5) || assessed > latest.AddMinutes(5) || assessed > utcNow.AddMinutes(5))
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentAssessedAtInvalid);

        if (relatedInterventionId.HasValue && !_interventions.Any(x => x.Id == relatedInterventionId.Value))
            return Result.Failure<SessionCompetencyAssessment>(TrainingSessionErrors.AssessmentInterventionNotFound);

        Result<SessionCompetencyAssessment> created = SessionCompetencyAssessment.Create(
            TrainingSessionCompetencyAssessmentId.New(), Id, operationId, fingerprint, competencyId, curriculumVersionId,
            pedagogyAssessmentId, levelCode, observedCriteria, context, relatedInterventionId, internalComment, sharedComment,
            evidenceDocumentId, assessed, actor, utcNow);
        if (created.IsFailure) return created;

        _competencyAssessments.Add(created.Value);
        LastModifiedAtUtc = utcNow;
        LastModifiedByUserId = actor;
        RaiseDomainEvent(new CompetencyAssessmentRecordedDomainEvent(
            Id, OrganizationId, TrainingPathId, created.Value.Id, competencyId, curriculumVersionId,
            pedagogyAssessmentId, created.Value.LevelCode, actor, assessed));
        return created;
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId) { CreatedAtUtc = createdAtUtc.ToUniversalTime(); CreatedByUserId = createdByUserId; }
    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId) { LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime(); LastModifiedByUserId = modifiedByUserId; }


    private Result FinalizeAttendanceAtCompletion(
        SessionAttendance current,
        Guid completionOperationId,
        DateTimeOffset actualEndAtUtc,
        UserId actor,
        DateTimeOffset now)
    {
        if (current.ActualDepartureAtUtc.HasValue)
        {
            if (current.ActualDepartureAtUtc.Value > actualEndAtUtc.AddMinutes(5))
                return Result.Failure(TrainingSessionErrors.CompletionAttendancePeriodInvalid);
            return Result.Success();
        }

        if (current.Status is not (TrainingSessionAttendanceStatus.Present or TrainingSessionAttendanceStatus.LateArrival))
            return Result.Success();

        Guid attendanceOperationId = BuildDeterministicOperationId(completionOperationId, "attendance-finalize");
        string requestFingerprint = BuildAttendanceFingerprint(
            current.Status, current.ActualArrivalAtUtc, actualEndAtUtc, current.Reason,
            current.EvidenceDocumentId, false, null);

        Result<SessionAttendance> finalized = SessionAttendance.Create(
            TrainingSessionAttendanceId.New(),
            Id,
            attendanceOperationId,
            requestFingerprint,
            current.Revision + 1,
            current.Status,
            current.ActualArrivalAtUtc,
            actualEndAtUtc,
            current.LateMinutes,
            current.Reason,
            current.EvidenceDocumentId,
            actor,
            now,
            current.Id,
            false,
            null);
        if (finalized.IsFailure) return Result.Failure(finalized.Error);

        _attendanceHistory.Add(finalized.Value);
        CurrentAttendanceId = finalized.Value.Id;
        RaiseDomainEvent(new TrainingSessionAttendanceCorrectedDomainEvent(
            Id, OrganizationId, current.Id, finalized.Value.Id, actor, now, false));
        RaiseAttendanceEvents(finalized.Value, actor, true);
        return Result.Success();
    }

    private Result FinalizeAttendanceAtTermination(SessionAttendance current, SessionCancellationId cancellationId, DateTimeOffset actualEndAtUtc, UserId actor, DateTimeOffset now)
    {
        if (current.ActualDepartureAtUtc.HasValue) return Result.Success();
        Guid attendanceOperationId = BuildDeterministicOperationId(cancellationId.Value, "attendance-cancellation-finalize");
        string requestFingerprint = BuildAttendanceFingerprint(current.Status, current.ActualArrivalAtUtc, actualEndAtUtc, current.Reason, current.EvidenceDocumentId, false, null);
        Result<SessionAttendance> finalized = SessionAttendance.Create(TrainingSessionAttendanceId.New(), Id, attendanceOperationId, requestFingerprint,
            current.Revision + 1, current.Status, current.ActualArrivalAtUtc, actualEndAtUtc, current.LateMinutes, current.Reason, current.EvidenceDocumentId,
            actor, now, current.Id, false, null);
        if (finalized.IsFailure) return Result.Failure(finalized.Error);
        _attendanceHistory.Add(finalized.Value);
        CurrentAttendanceId = finalized.Value.Id;
        RaiseDomainEvent(new TrainingSessionAttendanceCorrectedDomainEvent(Id, OrganizationId, current.Id, finalized.Value.Id, actor, now, false));
        RaiseAttendanceEvents(finalized.Value, actor, true);
        return Result.Success();
    }

    private decimal? CalculateSessionDistance()
    {
        SessionOdometerReading[] readings = _odometerReadings.OrderBy(x => x.ObservedAtUtc).ToArray();
        if (readings.Length < 2) return null;
        decimal distance = readings[^1].OdometerKilometers - readings[0].OdometerKilometers;
        return distance >= 0 ? decimal.Round(distance, 2, MidpointRounding.AwayFromZero) : null;
    }

    private static int RoundMinutes(TimeSpan duration) =>
        (int)Math.Round(duration.TotalMinutes, MidpointRounding.AwayFromZero);

    private static Guid BuildDeterministicOperationId(Guid operationId, string scope)
    {
        byte[] hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{operationId:D}|{scope}"));
        byte[] guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }

    private Result NormalizeAttendanceTimes(
        TrainingSessionAttendanceStatus status,
        ref DateTimeOffset? actualArrivalAtUtc,
        ref DateTimeOffset? actualDepartureAtUtc,
        DateTimeOffset utcNow,
        DateTimeOffset plannedStart,
        bool requireInProgress)
    {
        bool presence = status is TrainingSessionAttendanceStatus.Present or TrainingSessionAttendanceStatus.LateArrival or TrainingSessionAttendanceStatus.PartialAttendance;
        if (presence && requireInProgress && Status != TrainingSessionStatus.InProgress) return Result.Failure(TrainingSessionErrors.AttendanceRequiresStartedSession);
        if (presence && !requireInProgress && !ActualStartAtUtc.HasValue) return Result.Failure(TrainingSessionErrors.AttendanceRequiresStartedSession);

        if (presence)
        {
            actualArrivalAtUtc = (actualArrivalAtUtc ?? ActualStartAtUtc ?? utcNow).ToUniversalTime();
            actualDepartureAtUtc = actualDepartureAtUtc?.ToUniversalTime();
            if (status == TrainingSessionAttendanceStatus.LateArrival && actualArrivalAtUtc <= plannedStart)
                return Result.Failure(TrainingSessionErrors.AttendanceLateArrivalInvalid);
            if (status == TrainingSessionAttendanceStatus.PartialAttendance && !actualDepartureAtUtc.HasValue)
                return Result.Failure(TrainingSessionErrors.AttendancePartialPeriodRequired);
        }
        else
        {
            actualArrivalAtUtc = null;
            actualDepartureAtUtc = null;
        }

        return Result.Success();
    }

    private static string BuildOperationFingerprint(params object?[] values)
    {
        string payload = string.Join("|", values.Select(x => x?.ToString()?.Trim() ?? string.Empty));
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload)));
    }

    private static string BuildAttendanceFingerprint(
        TrainingSessionAttendanceStatus status,
        DateTimeOffset? actualArrivalAtUtc,
        DateTimeOffset? actualDepartureAtUtc,
        string? reason,
        Guid? evidenceDocumentId,
        bool isOverride,
        string? overrideReason)
    {
        string payload = string.Join("|",
            (int)status,
            actualArrivalAtUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
            actualDepartureAtUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
            reason?.Trim() ?? string.Empty,
            evidenceDocumentId?.ToString("D") ?? string.Empty,
            isOverride ? "1" : "0",
            overrideReason?.Trim() ?? string.Empty);
        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload)));
    }

    private static int CalculateLateMinutes(TrainingSessionAttendanceStatus status, DateTimeOffset? arrival, DateTimeOffset plannedStart)
    {
        if (status != TrainingSessionAttendanceStatus.LateArrival || !arrival.HasValue) return 0;
        return Math.Max(1, (int)Math.Ceiling((arrival.Value.ToUniversalTime() - plannedStart).TotalMinutes));
    }

    private void RaiseAttendanceEvents(SessionAttendance attendance, UserId actor, bool correction)
    {
        RaiseDomainEvent(new TrainingSessionAttendanceRecordedDomainEvent(Id, OrganizationId, attendance.Id, attendance.Status, actor, attendance.RecordedAtUtc));
        if (attendance.Status is TrainingSessionAttendanceStatus.StudentAbsent or TrainingSessionAttendanceStatus.ExcusedAbsence or TrainingSessionAttendanceStatus.UnexcusedAbsence)
            RaiseDomainEvent(new StudentAbsentRecordedDomainEvent(Id, OrganizationId, StudentId, attendance.Id, attendance.Status, attendance.RecordedAtUtc));
        if (attendance.Status == TrainingSessionAttendanceStatus.InstructorAbsent)
            RaiseDomainEvent(new InstructorAbsentRecordedDomainEvent(Id, OrganizationId, ActualInstructorId ?? ReadyInstructorId ?? InstructorId, attendance.Id, attendance.RecordedAtUtc));
    }

}

public sealed record TrainingSessionMaterialization(
    OrganizationId OrganizationId,
    OrganizationId StudentOwnerOrganizationId,
    OrganizationId PerformingOrganizationId,
    BookingId SourceBookingId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    UserId InstructorId,
    BranchId? BranchId,
    Guid? VehicleId,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    string? TrainingCategory,
    string? Objectives,
    string? MeetingPoint,
    string? PricingReference,
    TrainingCreditAccountId? TrainingCreditAccountId,
    decimal? CreditQuantity,
    string? CreditReservationReference);

public sealed record TrainingSessionReadinessSnapshot(
    bool IsReady,
    UserId InstructorId,
    BranchId? BranchId,
    Guid? VehicleId,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc);
