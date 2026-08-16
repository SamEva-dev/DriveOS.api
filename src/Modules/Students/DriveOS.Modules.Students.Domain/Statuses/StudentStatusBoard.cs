using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Statuses;

public sealed class StudentStatusBoard : AggregateRoot<StudentStatusBoardId>
{
    private readonly List<StudentOperationalBlock> blocks = [];
    private readonly List<StudentBlockHistory> history = [];

    private StudentStatusBoard() { }

    private StudentStatusBoard(StudentStatusBoardId id, OrganizationId org, PersonId studentId)
        : base(id)
    {
        OrganizationId = org;
        StudentId = studentId;
        FinancialStatus = FinancialStatus.Unknown;
        PedagogicalStatus = PedagogicalStatus.NotStarted;
        SchedulingStatus = SchedulingStatus.Allowed;
        ExamStatus = ExamStatus.NotReady;
        PortalAccessStatus = PortalAccessStatus.NotInvited;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentStatusBoardId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public FinancialStatus FinancialStatus { get; private set; }
    public PedagogicalStatus PedagogicalStatus { get; private set; }
    public SchedulingStatus SchedulingStatus { get; private set; }
    public ExamStatus ExamStatus { get; private set; }
    public PortalAccessStatus PortalAccessStatus { get; private set; }
    public IReadOnlyCollection<StudentOperationalBlock> Blocks => blocks;
    public IReadOnlyCollection<StudentBlockHistory> History => history;

    public static Result<StudentStatusBoard> Create(OrganizationId org, PersonId studentId) =>
        org.IsEmpty || studentId.IsEmpty
            ? Result.Failure<StudentStatusBoard>(StudentStatusErrors.InvalidOwner)
            : Result.Success(new StudentStatusBoard(StudentStatusBoardId.New(), org, studentId));

    public Result<Guid> ApplyBlock(
        string type,
        string reason,
        string source,
        StudentBlockingAction actions,
        StudentBlockSeverity severity,
        string expectedResolution,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (
            string.IsNullOrWhiteSpace(type)
            || string.IsNullOrWhiteSpace(reason)
            || string.IsNullOrWhiteSpace(source)
            || actions == StudentBlockingAction.None
        )
            return Result.Failure<Guid>(StudentStatusErrors.InvalidBlock);
        var block = StudentOperationalBlock.Create(
            Id,
            type.Trim(),
            reason.Trim(),
            source.Trim(),
            actions,
            severity,
            expectedResolution?.Trim() ?? string.Empty,
            actor,
            now
        );
        blocks.Add(block);
        AddHistory(block.Id, "Applied", reason, actor, now);
        return Result.Success(block.Id);
    }

    public Result Release(
        Guid id,
        StudentBlockResolutionType resolution,
        string reason,
        UserId actor,
        DateTimeOffset now,
        bool automatic = false
    )
    {
        var block = blocks.SingleOrDefault(x => x.Id == id);
        if (block is null)
            return Result.Failure(StudentStatusErrors.BlockNotFound);
        if (!block.IsUnresolved())
            return Result.Failure(StudentStatusErrors.BlockNotActive);
        if (!automatic && string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentStatusErrors.ReasonRequired);
        block.Release(resolution, reason?.Trim() ?? "Automatic event", actor, now, automatic);
        AddHistory(id, automatic ? "ResolvedAutomatically" : "Released", reason, actor, now);
        return Result.Success();
    }

    public Result Override(
        Guid id,
        string reason,
        DateTimeOffset until,
        UserId actor,
        DateTimeOffset now
    )
    {
        var block = blocks.SingleOrDefault(x => x.Id == id);
        if (block is null)
            return Result.Failure(StudentStatusErrors.BlockNotFound);
        if (!block.IsEffectivelyActive(now))
            return Result.Failure(StudentStatusErrors.BlockNotActive);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentStatusErrors.ReasonRequired);
        if (until <= now || until > now.AddYears(1))
            return Result.Failure(StudentStatusErrors.OverridePeriodInvalid);
        block.Override(reason.Trim(), until, actor, now);
        AddHistory(id, "Overridden", reason, actor, now);
        return Result.Success();
    }

    public void ProjectStatuses(
        FinancialStatus financial,
        PedagogicalStatus pedagogical,
        SchedulingStatus scheduling,
        ExamStatus exam,
        PortalAccessStatus portal
    )
    {
        FinancialStatus = financial;
        PedagogicalStatus = pedagogical;
        SchedulingStatus = scheduling;
        ExamStatus = exam;
        PortalAccessStatus = portal;
    }

    private void AddHistory(
        Guid blockId,
        string action,
        string? detail,
        UserId actor,
        DateTimeOffset now
    ) =>
        history.Add(
            StudentBlockHistory.Create(
                Id,
                blockId,
                action,
                detail?.Trim() ?? string.Empty,
                actor,
                now
            )
        );
}

public sealed class StudentOperationalBlock
{
    private StudentOperationalBlock() { }

    public Guid Id { get; private set; }
    public StudentStatusBoardId StudentStatusBoardId { get; private set; }
    public string BlockType { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string SourceDomain { get; private set; } = string.Empty;
    public StudentBlockingAction BlockingActions { get; private set; }
    public StudentBlockSeverity Severity { get; private set; }
    public DateTimeOffset AppliedAtUtc { get; private set; }
    public UserId AppliedByUserId { get; private set; }
    public string ExpectedResolution { get; private set; } = string.Empty;
    public StudentBlockStatus Status { get; private set; }
    public StudentBlockResolutionType? ResolutionType { get; private set; }
    public string? ResolutionReason { get; private set; }
    public DateTimeOffset? ResolvedAtUtc { get; private set; }
    public UserId? ResolvedByUserId { get; private set; }
    public DateTimeOffset? OverrideUntilUtc { get; private set; }
    public string? OverrideReason { get; private set; }
    public UserId? OverrideByUserId { get; private set; }

    internal static StudentOperationalBlock Create(
        Guid board,
        string type,
        string reason,
        string source,
        StudentBlockingAction actions,
        StudentBlockSeverity severity,
        string expected,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentStatusBoardId = new StudentStatusBoardId(board),
            BlockType = type,
            Reason = reason,
            SourceDomain = source,
            BlockingActions = actions,
            Severity = severity,
            ExpectedResolution = expected,
            AppliedAtUtc = now,
            AppliedByUserId = actor,
            Status = StudentBlockStatus.Active,
        };

    internal bool IsEffectivelyActive(DateTimeOffset now) =>
        Status == StudentBlockStatus.Active
        || (Status == StudentBlockStatus.Overridden && OverrideUntilUtc <= now);

    internal bool IsUnresolved() =>
        Status is StudentBlockStatus.Active or StudentBlockStatus.Overridden;

    internal void Override(string reason, DateTimeOffset until, UserId actor, DateTimeOffset now)
    {
        Status = StudentBlockStatus.Overridden;
        OverrideReason = reason;
        OverrideUntilUtc = until;
        OverrideByUserId = actor;
        ResolutionType = StudentBlockResolutionType.TemporaryOverride;
    }

    internal void Release(
        StudentBlockResolutionType resolution,
        string reason,
        UserId actor,
        DateTimeOffset now,
        bool automatic
    )
    {
        Status = automatic ? StudentBlockStatus.Resolved : StudentBlockStatus.Released;
        ResolutionType = resolution;
        ResolutionReason = reason;
        ResolvedAtUtc = now;
        ResolvedByUserId = actor;
    }
}

public sealed class StudentBlockHistory
{
    private StudentBlockHistory() { }

    public Guid Id { get; private set; }
    public StudentStatusBoardId StudentStatusBoardId { get; private set; }
    public Guid BlockId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string Detail { get; private set; } = string.Empty;
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    internal static StudentBlockHistory Create(
        Guid board,
        Guid block,
        string action,
        string detail,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentStatusBoardId = new StudentStatusBoardId(board),
            BlockId = block,
            Action = action,
            Detail = detail,
            ActorUserId = actor,
            OccurredAtUtc = now,
        };
}
