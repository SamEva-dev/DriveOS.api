using DriveOS.Modules.Students.Domain.Events;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Students.Domain.Suspensions;

public sealed class EnrollmentSuspension : AggregateRoot<EnrollmentSuspensionId>
{
    private readonly List<EnrollmentSuspensionHistory> history = [];

    private EnrollmentSuspension() { }

    private EnrollmentSuspension(
        EnrollmentSuspensionId id,
        OrganizationId org,
        PersonId student,
        DraftEnrollmentId enrollment,
        EnrollmentSuspensionReason reason,
        EnrollmentSuspensionScope scope,
        DateOnly start,
        DateOnly expectedEnd,
        string immediateActions,
        ExistingBookingsDecision bookings,
        int futureBookings,
        string creditDecision,
        string notificationPlan,
        DateOnly review,
        UserId actor,
        DateTimeOffset now
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = student;
        EnrollmentId = enrollment;
        Reason = reason;
        Scope = scope;
        StartDate = start;
        ExpectedEndDate = expectedEnd;
        ImmediateActions = immediateActions;
        BookingsDecision = bookings;
        FutureBookingsCount = futureBookings;
        CreditDecision = creditDecision;
        NotificationPlan = notificationPlan;
        ReviewDate = review;
        Status =
            start > DateOnly.FromDateTime(now.UtcDateTime)
                ? EnrollmentSuspensionStatus.Scheduled
                : EnrollmentSuspensionStatus.Active;
        NotificationStatus = SuspensionNotificationStatus.Queued;
        CreatedAtUtc = now;
        CreatedByUserId = actor;
        history.Add(
            EnrollmentSuspensionHistory.Create(id, "Created", reason.ToString(), actor, now)
        );
        RaiseDomainEvent(
            new StudentAggregateChangedDomainEvent<EnrollmentSuspensionId>(
                Id,
                StudentId,
                OrganizationId,
                "Created"
            )
        );
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public DraftEnrollmentId EnrollmentId { get; private set; }
    public EnrollmentSuspensionReason Reason { get; private set; }
    public EnrollmentSuspensionScope Scope { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly ExpectedEndDate { get; private set; }
    public string ImmediateActions { get; private set; } = string.Empty;
    public ExistingBookingsDecision BookingsDecision { get; private set; }
    public int FutureBookingsCount { get; private set; }
    public string CreditDecision { get; private set; } = string.Empty;
    public string NotificationPlan { get; private set; } = string.Empty;
    public DateOnly ReviewDate { get; private set; }
    public EnrollmentSuspensionStatus Status { get; private set; }
    public SuspensionNotificationStatus NotificationStatus { get; private set; }
    public Guid? OperationalBlockId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? ActivatedAtUtc { get; private set; }
    public IReadOnlyCollection<EnrollmentSuspensionHistory> History => history;

    public static Result<EnrollmentSuspension> Create(
        OrganizationId org,
        PersonId student,
        DraftEnrollmentId enrollment,
        EnrollmentSuspensionReason reason,
        EnrollmentSuspensionScope scope,
        DateOnly start,
        DateOnly expectedEnd,
        string immediateActions,
        ExistingBookingsDecision bookings,
        int futureBookings,
        string creditDecision,
        string notificationPlan,
        DateOnly review,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (org.IsEmpty || student.IsEmpty || enrollment.IsEmpty)
            return Result.Failure<EnrollmentSuspension>(EnrollmentSuspensionErrors.InvalidOwner);
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        bool invalidScope =
            scope == EnrollmentSuspensionScope.None
            || (scope & ~EnrollmentSuspensionScope.All) != 0
            || (
                scope.HasFlag(EnrollmentSuspensionScope.FullEnrollment)
                && scope != EnrollmentSuspensionScope.FullEnrollment
            );
        if (
            invalidScope
            || start < today
            || expectedEnd <= start
            || review < start
            || review > expectedEnd
            || futureBookings < 0
            || string.IsNullOrWhiteSpace(immediateActions)
            || string.IsNullOrWhiteSpace(creditDecision)
            || string.IsNullOrWhiteSpace(notificationPlan)
        )
            return Result.Failure<EnrollmentSuspension>(EnrollmentSuspensionErrors.InvalidRequest);
        if (
            futureBookings > 0
            && bookings == ExistingBookingsDecision.Keep
            && scope.HasFlag(EnrollmentSuspensionScope.SchedulingOnly)
        )
            return Result.Failure<EnrollmentSuspension>(
                EnrollmentSuspensionErrors.FutureBookingsUntreated
            );
        return Result.Success(
            new EnrollmentSuspension(
                EnrollmentSuspensionId.New(),
                org,
                student,
                enrollment,
                reason,
                scope,
                start,
                expectedEnd,
                immediateActions.Trim(),
                bookings,
                futureBookings,
                creditDecision.Trim(),
                notificationPlan.Trim(),
                review,
                actor,
                now
            )
        );
    }

    public Result Activate(Guid blockId, UserId actor, DateTimeOffset now)
    {
        if (Status != EnrollmentSuspensionStatus.Scheduled)
            return Result.Failure(EnrollmentSuspensionErrors.InvalidTransition);
        Status = EnrollmentSuspensionStatus.Active;
        OperationalBlockId = blockId;
        ActivatedAtUtc = now;
        history.Add(
            EnrollmentSuspensionHistory.Create(Id, "Activated", Scope.ToString(), actor, now)
        );
        RaiseDomainEvent(
            new StudentAggregateChangedDomainEvent<EnrollmentSuspensionId>(
                Id,
                StudentId,
                OrganizationId,
                "Activated"
            )
        );
        return Result.Success();
    }

    public void AttachBlock(Guid blockId, UserId actor, DateTimeOffset now)
    {
        OperationalBlockId = blockId;
        ActivatedAtUtc = now;
        history.Add(
            EnrollmentSuspensionHistory.Create(
                Id,
                "OperationalBlockApplied",
                Scope.ToString(),
                actor,
                now
            )
        );
    }

    public Result EndForReactivation(UserId actor, DateTimeOffset now)
    {
        if (Status != EnrollmentSuspensionStatus.Active)
            return Result.Failure(EnrollmentSuspensionErrors.InvalidTransition);
        Status = EnrollmentSuspensionStatus.Ended;
        history.Add(
            EnrollmentSuspensionHistory.Create(
                Id,
                "Reactivated",
                "Suspension ended without restoring prior grants",
                actor,
                now
            )
        );
        RaiseDomainEvent(
            new StudentAggregateChangedDomainEvent<EnrollmentSuspensionId>(
                Id,
                StudentId,
                OrganizationId,
                "Ended"
            )
        );
        return Result.Success();
    }

    public void MarkNotificationSent(UserId actor, DateTimeOffset now)
    {
        NotificationStatus = SuspensionNotificationStatus.Sent;
        history.Add(
            EnrollmentSuspensionHistory.Create(Id, "NotificationSent", NotificationPlan, actor, now)
        );
    }
}

public sealed class EnrollmentSuspensionHistory
{
    private EnrollmentSuspensionHistory() { }

    public Guid Id { get; private set; }
    public EnrollmentSuspensionId EnrollmentSuspensionId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    internal static EnrollmentSuspensionHistory Create(
        EnrollmentSuspensionId suspension,
        string action,
        string detail,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            EnrollmentSuspensionId = suspension,
            Action = action,
            Detail = detail,
            ActorUserId = actor,
            OccurredAtUtc = now,
        };
}

public sealed class EnrollmentReactivation : AggregateRoot<EnrollmentReactivationId>
{
    private readonly List<EnrollmentReactivationCheck> checks = [];

    private EnrollmentReactivation() { }

    private EnrollmentReactivation(
        EnrollmentReactivationId id,
        OrganizationId org,
        PersonId student,
        DraftEnrollmentId enrollment,
        EnrollmentSuspensionId suspension,
        EnrollmentReactivationMode mode,
        DateOnly resumeDate,
        string conditions,
        bool pedagogyReview,
        UserId actor,
        DateTimeOffset now,
        IEnumerable<EnrollmentReactivationCheckSeed> seeds
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = student;
        EnrollmentId = enrollment;
        SuspensionId = suspension;
        Mode = mode;
        ResumeDate = resumeDate;
        Conditions = conditions;
        PedagogyReviewRequested = pedagogyReview;
        CreatedByUserId = actor;
        CreatedAtUtc = now;
        checks.AddRange(
            seeds.Select(x => EnrollmentReactivationCheck.Create(id, x.Type, x.Status, x.Detail))
        );
        Status =
            mode == EnrollmentReactivationMode.NewEnrollment
                ? EnrollmentReactivationStatus.NewEnrollmentRequired
            : mode == EnrollmentReactivationMode.Scheduled ? EnrollmentReactivationStatus.Scheduled
            : EnrollmentReactivationStatus.PendingConditions;
        RaiseDomainEvent(
            new StudentAggregateChangedDomainEvent<EnrollmentReactivationId>(
                Id,
                StudentId,
                OrganizationId,
                "Created"
            )
        );
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public DraftEnrollmentId EnrollmentId { get; private set; }
    public EnrollmentSuspensionId SuspensionId { get; private set; }
    public EnrollmentReactivationMode Mode { get; private set; }
    public DateOnly ResumeDate { get; private set; }
    public string Conditions { get; private set; } = string.Empty;
    public bool PedagogyReviewRequested { get; private set; }
    public EnrollmentReactivationStatus Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }
    public IReadOnlyCollection<EnrollmentReactivationCheck> Checks => checks;

    public static Result<EnrollmentReactivation> Create(
        OrganizationId org,
        PersonId student,
        DraftEnrollmentId enrollment,
        EnrollmentSuspensionId suspension,
        EnrollmentReactivationMode mode,
        DateOnly resumeDate,
        string conditions,
        bool pedagogyReview,
        UserId actor,
        DateTimeOffset now,
        IEnumerable<EnrollmentReactivationCheckSeed> seeds
    )
    {
        var list = seeds.ToArray();
        var required = Enum.GetValues<ReactivationCheckType>();
        bool complete =
            required.All(type => list.Count(x => x.Type == type) == 1)
            && list.Length == required.Length;
        if (
            org.IsEmpty
            || student.IsEmpty
            || enrollment.IsEmpty
            || suspension == Guid.Empty
            || resumeDate < DateOnly.FromDateTime(now.UtcDateTime)
            || !complete
        )
            return Result.Failure<EnrollmentReactivation>(
                EnrollmentSuspensionErrors.InvalidRequest
            );
        bool failed = list.Any(x => x.Status == ReactivationCheckStatus.Failed);
        if (
            (mode is EnrollmentReactivationMode.Immediate or EnrollmentReactivationMode.Scheduled)
            && failed
        )
            return Result.Failure<EnrollmentReactivation>(
                EnrollmentSuspensionErrors.ReactivationChecksIncomplete
            );
        if (mode == EnrollmentReactivationMode.Conditional && string.IsNullOrWhiteSpace(conditions))
            return Result.Failure<EnrollmentReactivation>(
                EnrollmentSuspensionErrors.InvalidRequest
            );
        return Result.Success(
            new EnrollmentReactivation(
                EnrollmentReactivationId.New(),
                org,
                student,
                enrollment,
                suspension,
                mode,
                resumeDate,
                conditions?.Trim() ?? string.Empty,
                pedagogyReview,
                actor,
                now,
                list
            )
        );
    }

    public Result ReviewCheck(
        ReactivationCheckType type,
        ReactivationCheckStatus status,
        string detail
    )
    {
        if (
            Status
            is EnrollmentReactivationStatus.Applied
                or EnrollmentReactivationStatus.NewEnrollmentRequired
        )
            return Result.Failure(EnrollmentSuspensionErrors.InvalidTransition);
        var check = checks.SingleOrDefault(x => x.Type == type);
        if (check is null)
            return Result.Failure(EnrollmentSuspensionErrors.InvalidRequest);
        check.Review(status, detail);
        return Result.Success();
    }

    public Result Apply(DateOnly today, DateTimeOffset now)
    {
        if (
            Status
                is EnrollmentReactivationStatus.Applied
                    or EnrollmentReactivationStatus.NewEnrollmentRequired
            || ResumeDate > today
            || Checks.Any(x => x.Status == ReactivationCheckStatus.Failed)
        )
            return Result.Failure(EnrollmentSuspensionErrors.ReactivationChecksIncomplete);
        Status = EnrollmentReactivationStatus.Applied;
        AppliedAtUtc = now;
        RaiseDomainEvent(
            new StudentAggregateChangedDomainEvent<EnrollmentReactivationId>(
                Id,
                StudentId,
                OrganizationId,
                "Applied"
            )
        );
        return Result.Success();
    }
}

public sealed record EnrollmentReactivationCheckSeed(
    ReactivationCheckType Type,
    ReactivationCheckStatus Status,
    string Detail
);

public sealed class EnrollmentReactivationCheck
{
    private EnrollmentReactivationCheck() { }

    public Guid Id { get; private set; }
    public EnrollmentReactivationId EnrollmentReactivationId { get; private set; }
    public ReactivationCheckType Type { get; private set; }
    public ReactivationCheckStatus Status { get; private set; }
    public string Detail { get; private set; } = string.Empty;

    internal static EnrollmentReactivationCheck Create(
        EnrollmentReactivationId reactivation,
        ReactivationCheckType type,
        ReactivationCheckStatus status,
        string detail
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            EnrollmentReactivationId = reactivation,
            Type = type,
            Status = status,
            Detail = detail?.Trim() ?? string.Empty,
        };

    internal void Review(ReactivationCheckStatus status, string detail)
    {
        Status = status;
        Detail = detail?.Trim() ?? string.Empty;
    }
}
