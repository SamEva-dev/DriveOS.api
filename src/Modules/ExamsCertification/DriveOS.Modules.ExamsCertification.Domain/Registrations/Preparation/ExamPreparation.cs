using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;

/// <summary>
/// Owns the versioned operational readiness snapshot immediately before an examination.
/// Authoritative facts remain owned by their source bounded contexts; this aggregate stores only
/// the evaluated evidence required to hand a stable, auditable preparation revision to the day-of-exam workflow.
/// </summary>
public sealed class ExamPreparation : AggregateRoot<ExamPreparationId>, IAuditableEntity
{
    private readonly List<ExamPreparationCheck> _checks = [];
    private readonly List<int> _reminderOffsetsDays = [];

    private ExamPreparation() { }

    private ExamPreparation(
        ExamPreparationId id,
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        PersonId studentId,
        UserId actor,
        DateTimeOffset now) : base(id)
    {
        OrganizationId = organizationId;
        RegistrationId = registrationId;
        StudentId = studentId;
        Status = ExamPreparationStatus.Incomplete;
        CreatedAtUtc = now.ToUniversalTime();
        CreatedByUserId = actor;
    }

    public OrganizationId OrganizationId { get; private set; }
    public ExamRegistrationId RegistrationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public int Revision { get; private set; }
    public int ConvocationVersion { get; private set; }
    public ExamPreparationStatus Status { get; private set; }
    public bool MeetingPointConfirmed { get; private set; }
    public bool VehicleEnergyConfirmed { get; private set; }
    public bool InstructorConfirmed { get; private set; }
    public bool InstructionsTransmitted { get; private set; }
    public Guid? LastOperationId { get; private set; }
    public string? LastRequestFingerprint { get; private set; }
    public DateTimeOffset? LastEvaluatedAtUtc { get; private set; }

    /// <summary>
    /// Revision that was explicitly confirmed by a human operator. A refresh increments <see cref="Revision"/>,
    /// therefore a previous confirmation automatically becomes stale without deleting its audit information.
    /// </summary>
    public int? ConfirmedRevision { get; private set; }
    public DateTimeOffset? ConfirmedAtUtc { get; private set; }
    public UserId? ConfirmedByUserId { get; private set; }
    public bool IsConfirmed => Status == ExamPreparationStatus.Ready && ConfirmedRevision == Revision;

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }
    public IReadOnlyCollection<ExamPreparationCheck> Checks => _checks.AsReadOnly();
    public IReadOnlyCollection<int> ReminderOffsetsDays => _reminderOffsetsDays.AsReadOnly();

    public static Result<ExamPreparation> Create(
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        PersonId studentId,
        UserId actor,
        DateTimeOffset now)
    {
        if (organizationId.IsEmpty || registrationId.IsEmpty || studentId.IsEmpty || actor.IsEmpty)
            return Result.Failure<ExamPreparation>(ExamPreparationErrors.InvalidIdentifier);

        var preparation = new ExamPreparation(
            ExamPreparationId.New(), organizationId, registrationId, studentId, actor, now);

        preparation.RaiseDomainEvent(new ExamPreparationCreatedDomainEvent(
            preparation.Id, organizationId, registrationId));

        return Result.Success(preparation);
    }

    public Result Refresh(
        int convocationVersion,
        IReadOnlyCollection<ExamPreparationCheckSnapshot> sourceChecks,
        bool instructorRequired,
        bool vehicleRequired,
        bool meetingPointConfirmed,
        bool vehicleEnergyConfirmed,
        bool instructorConfirmed,
        bool instructionsTransmitted,
        IReadOnlyCollection<int> reminderOffsetsDays,
        Guid operationId,
        string requestFingerprint,
        UserId actor,
        DateTimeOffset now)
    {
        if (convocationVersion <= 0
            || sourceChecks.Count == 0
            || sourceChecks.Any(x => string.IsNullOrWhiteSpace(x.Code)
                || string.IsNullOrWhiteSpace(x.MessageKey)
                || string.IsNullOrWhiteSpace(x.Source)))
            return Result.Failure(ExamPreparationErrors.InvalidSnapshot);

        if (operationId == Guid.Empty || actor.IsEmpty || string.IsNullOrWhiteSpace(requestFingerprint))
            return Result.Failure(ExamPreparationErrors.InvalidIdentifier);

        if (reminderOffsetsDays.Count == 0
            || reminderOffsetsDays.Any(x => x < 0 || x > 30))
            return Result.Failure(ExamPreparationErrors.InvalidSnapshot);

        if (sourceChecks.GroupBy(x => x.Code, StringComparer.Ordinal).Any(g => g.Count() > 1))
            return Result.Failure(ExamPreparationErrors.DuplicateCheckCode);

        if (LastOperationId == operationId)
        {
            return string.Equals(LastRequestFingerprint, requestFingerprint, StringComparison.Ordinal)
                ? Result.Success()
                : Result.Failure(ExamPreparationErrors.OperationConflict);
        }

        int previousRevision = Revision;
        bool previousRevisionWasConfirmed = IsConfirmed;
        int previousConvocationVersion = ConvocationVersion;
        string[] previousBlocking = _checks
            .Where(x => x.Required && x.Status == ExamPreparationCheckStatus.Blocked)
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToArray();

        MeetingPointConfirmed = meetingPointConfirmed;
        VehicleEnergyConfirmed = vehicleEnergyConfirmed;
        InstructorConfirmed = instructorConfirmed;
        InstructionsTransmitted = instructionsTransmitted;

        _reminderOffsetsDays.Clear();
        _reminderOffsetsDays.AddRange(reminderOffsetsDays.Distinct().OrderByDescending(x => x));

        ConvocationVersion = convocationVersion;
        Revision++;
        LastOperationId = operationId;
        LastRequestFingerprint = requestFingerprint;
        LastEvaluatedAtUtc = now.ToUniversalTime();
        SetModifiedAudit(now, actor);

        _checks.Clear();
        foreach (ExamPreparationCheckSnapshot item in sourceChecks)
            _checks.Add(ExamPreparationCheck.Create(Id, item));

        AddManualCheck(
            "MeetingPointConfirmed",
            true,
            meetingPointConfirmed,
            "exams.preparation.meetingPoint",
            "ExamsCertification");

        AddManualCheck(
            "VehicleEnergyConfirmed",
            vehicleRequired,
            vehicleEnergyConfirmed,
            "exams.preparation.vehicleEnergy",
            "FleetResources");

        AddManualCheck(
            "InstructorConfirmed",
            instructorRequired,
            instructorConfirmed,
            "exams.preparation.instructorConfirmed",
            "ExamsCertification");

        AddManualCheck(
            "InstructionsTransmitted",
            true,
            instructionsTransmitted,
            "exams.preparation.instructionsTransmitted",
            "ExamsCertification");

        bool blocked = _checks.Any(x => x.Required && x.Status == ExamPreparationCheckStatus.Blocked);
        bool ready = _checks
            .Where(x => x.Required)
            .All(x => x.Status is ExamPreparationCheckStatus.Ready or ExamPreparationCheckStatus.NotApplicable);

        Status = blocked
            ? ExamPreparationStatus.Blocked
            : ready
                ? ExamPreparationStatus.Ready
                : ExamPreparationStatus.Incomplete;

        RaiseDomainEvent(new ExamPreparationRefreshedDomainEvent(
            Id, OrganizationId, RegistrationId, Revision, Status == ExamPreparationStatus.Ready));

        if (previousRevisionWasConfirmed)
        {
            RaiseDomainEvent(new ExamPreparationConfirmationInvalidatedDomainEvent(
                Id, OrganizationId, RegistrationId, previousRevision, Revision));
        }

        string[] currentBlocking = _checks
            .Where(x => x.Required && x.Status == ExamPreparationCheckStatus.Blocked)
            .Select(x => x.Code)
            .OrderBy(x => x)
            .ToArray();

        string[] urgent = currentBlocking.Except(previousBlocking, StringComparer.Ordinal).ToArray();
        if (Revision > 1 && (urgent.Length > 0 || previousConvocationVersion != convocationVersion))
        {
            var changes = urgent.ToList();
            if (previousConvocationVersion != convocationVersion)
                changes.Add("ConvocationVersionChanged");

            RaiseDomainEvent(new ExamPreparationUrgentChangeDetectedDomainEvent(
                Id, OrganizationId, RegistrationId, changes));
        }

        return Result.Success();
    }

    public Result Confirm(UserId actor, DateTimeOffset now)
    {
        if (actor.IsEmpty)
            return Result.Failure(ExamPreparationErrors.InvalidIdentifier);

        if (Status != ExamPreparationStatus.Ready || Revision <= 0)
            return Result.Failure(ExamPreparationErrors.NotReadyForConfirmation);

        if (IsConfirmed)
            return Result.Success();

        ConfirmedRevision = Revision;
        ConfirmedAtUtc = now.ToUniversalTime();
        ConfirmedByUserId = actor;
        SetModifiedAudit(now, actor);

        RaiseDomainEvent(new ExamPreparationConfirmedDomainEvent(
            Id, OrganizationId, RegistrationId, Revision, actor));

        return Result.Success();
    }

    private void AddManualCheck(string code, bool required, bool confirmed, string messageKey, string source) =>
        _checks.Add(ExamPreparationCheck.Create(
            Id,
            new ExamPreparationCheckSnapshot(
                code,
                required,
                !required
                    ? ExamPreparationCheckStatus.NotApplicable
                    : confirmed
                        ? ExamPreparationCheckStatus.Ready
                        : ExamPreparationCheckStatus.Pending,
                messageKey,
                source,
                confirmed ? "confirmed" : null)));

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        CreatedAtUtc = createdAtUtc.ToUniversalTime();
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc.ToUniversalTime();
        LastModifiedByUserId = modifiedByUserId;
    }
}

public sealed class ExamPreparationCheck
{
    private ExamPreparationCheck() { }

    public Guid Id { get; private set; }
    public ExamPreparationId PreparationId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public bool Required { get; private set; }
    public ExamPreparationCheckStatus Status { get; private set; }
    public string MessageKey { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public string? Evidence { get; private set; }
    public bool? IsConfirmed { get; set; }

    internal static ExamPreparationCheck Create(
        ExamPreparationId preparationId,
        ExamPreparationCheckSnapshot item) => new()
    {
        Id = Guid.NewGuid(),
        PreparationId = preparationId,
        Code = item.Code.Trim(),
        Required = item.Required,
        Status = item.Status,
        MessageKey = item.MessageKey.Trim(),
        Source = item.Source.Trim(),
        Evidence = string.IsNullOrWhiteSpace(item.Evidence) ? null : item.Evidence.Trim()
    };
}

public sealed record ExamPreparationCheckSnapshot(
    string Code,
    bool Required,
    ExamPreparationCheckStatus Status,
    string MessageKey,
    string Source,
    string? Evidence = null);
