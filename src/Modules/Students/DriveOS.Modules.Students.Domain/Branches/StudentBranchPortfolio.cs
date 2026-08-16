using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Branches;

public sealed class StudentBranchPortfolio : AggregateRoot<StudentBranchPortfolioId>
{
    private readonly List<StudentBranchAssignment> assignments = [];
    private readonly List<PrimaryBranchChangeAnalysis> analyses = [];

    private StudentBranchPortfolio() { }

    private StudentBranchPortfolio(StudentBranchPortfolioId id, OrganizationId org, PersonId studentId)
        : base(id)
    {
        OrganizationId = org;
        StudentId = studentId;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<StudentBranchPortfolioId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public IReadOnlyCollection<StudentBranchAssignment> Assignments => assignments;
    public IReadOnlyCollection<PrimaryBranchChangeAnalysis> Analyses => analyses;

    public static Result<StudentBranchPortfolio> Create(OrganizationId org, PersonId student) =>
        org.IsEmpty || student.IsEmpty
            ? Result.Failure<StudentBranchPortfolio>(StudentBranchErrors.InvalidOwner)
            : Result.Success(new StudentBranchPortfolio(StudentBranchPortfolioId.New(), org, student));

    public Result<Guid> Assign(
        BranchId branch,
        StudentBranchAssignmentType type,
        StudentBranchService services,
        DateOnly from,
        DateOnly? to,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (branch.IsEmpty || to < from)
            return Result.Failure<Guid>(StudentBranchErrors.InvalidPeriod);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure<Guid>(StudentBranchErrors.ReasonRequired);
        if (
            type == StudentBranchAssignmentType.Primary
            && assignments.Any(x =>
                x.Type == type
                && x.Status
                    is StudentBranchAssignmentStatus.Active
                        or StudentBranchAssignmentStatus.Planned
            )
        )
            return Result.Failure<Guid>(StudentBranchErrors.PrimaryAlreadyExists);
        var item = StudentBranchAssignment.Create(
            Id,
            branch,
            type,
            services,
            from,
            to,
            reason.Trim(),
            actor,
            now
        );
        assignments.Add(item);
        return Result.Success(item.Id);
    }

    public PrimaryBranchChangeAnalysis AnalyzePrimaryChange(
        BranchId target,
        IReadOnlyList<BranchChangeImpact> impacts,
        UserId actor,
        DateTimeOffset now
    )
    {
        var current = assignments.SingleOrDefault(x =>
            x.Type == StudentBranchAssignmentType.Primary
            && x.Status
                is StudentBranchAssignmentStatus.Active
                    or StudentBranchAssignmentStatus.Planned
        );
        var analysis = PrimaryBranchChangeAnalysis.Create(
            Id,
            current?.BranchId,
            target,
            impacts,
            actor,
            now
        );
        analyses.Add(analysis);
        return analysis;
    }

    public Result ChangePrimary(Guid analysisId, string reason, UserId actor, DateTimeOffset now)
    {
        var analysis = analyses.SingleOrDefault(x => x.Id == analysisId);
        if (analysis is null)
            return Result.Failure(StudentBranchErrors.AnalysisRequired);
        if (!analysis.CanApply(now))
            return Result.Failure(StudentBranchErrors.AnalysisExpired);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentBranchErrors.ReasonRequired);
        foreach (
            var item in assignments.Where(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && x.Status
                    is StudentBranchAssignmentStatus.Active
                        or StudentBranchAssignmentStatus.Planned
            )
        )
            item.End(DateOnly.FromDateTime(now.UtcDateTime), actor, now);
        assignments.Add(
            StudentBranchAssignment.Create(
                Id,
                analysis.TargetBranchId,
                StudentBranchAssignmentType.Primary,
                StudentBranchService.None,
                DateOnly.FromDateTime(now.UtcDateTime),
                null,
                reason.Trim(),
                actor,
                now
            )
        );
        analysis.MarkApplied(actor, now);
        return Result.Success();
    }

    public Result End(Guid id, string reason, UserId actor, DateTimeOffset now)
    {
        var item = assignments.SingleOrDefault(x => x.Id == id);
        if (item is null)
            return Result.Failure(StudentBranchErrors.AssignmentNotFound);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(StudentBranchErrors.ReasonRequired);
        item.End(DateOnly.FromDateTime(now.UtcDateTime), actor, now);
        return Result.Success();
    }

    public Result TransferPrimary(
        BranchId target,
        DateOnly effectiveOn,
        DateOnly? temporaryUntil,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (
            target.IsEmpty
            || string.IsNullOrWhiteSpace(reason)
            || temporaryUntil.HasValue && temporaryUntil.Value <= effectiveOn
        )
            return Result.Failure(StudentBranchErrors.InvalidPeriod);
        var current = assignments
            .Where(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && (
                    x.Status
                    is StudentBranchAssignmentStatus.Active
                        or StudentBranchAssignmentStatus.Planned
                )
            )
            .OrderBy(x => x.EffectiveFrom)
            .FirstOrDefault();
        var source = current?.BranchId;
        var sourceEndsOn = effectiveOn.AddDays(-1);
        foreach (
            var item in assignments.Where(x =>
                x.Type == StudentBranchAssignmentType.Primary
                && (
                    x.Status
                    is StudentBranchAssignmentStatus.Active
                        or StudentBranchAssignmentStatus.Planned
                )
            )
        )
            item.ScheduleEnd(sourceEndsOn, actor, now);
        var services =
            StudentBranchService.TheoryCourse
            | StudentBranchService.PracticalLesson
            | StudentBranchService.Simulator
            | StudentBranchService.ExamSupport
            | StudentBranchService.Administration;
        assignments.Add(
            StudentBranchAssignment.Create(
                Id,
                target,
                StudentBranchAssignmentType.Primary,
                services,
                effectiveOn,
                temporaryUntil,
                reason.Trim(),
                actor,
                now
            )
        );
        if (temporaryUntil.HasValue && source.HasValue)
            assignments.Add(
                StudentBranchAssignment.Create(
                    Id,
                    source.Value,
                    StudentBranchAssignmentType.Primary,
                    services,
                    temporaryUntil.Value.AddDays(1),
                    null,
                    $"Temporary transfer return: {reason.Trim()}",
                    actor,
                    now
                )
            );
        return Result.Success();
    }
}

public sealed class StudentBranchAssignment
{
    private StudentBranchAssignment() { }

    public Guid Id { get; private set; }
    public StudentBranchPortfolioId StudentBranchPortfolioId { get; private set; }
    public BranchId BranchId { get; private set; }
    public StudentBranchAssignmentType Type { get; private set; }
    public StudentBranchService ServicesAllowed { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public StudentBranchAssignmentStatus Status { get; private set; }
    public UserId CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? EndedByUserId { get; private set; }
    public DateTimeOffset? EndedAtUtc { get; private set; }

    internal static StudentBranchAssignment Create(
        Guid board,
        BranchId branch,
        StudentBranchAssignmentType type,
        StudentBranchService services,
        DateOnly from,
        DateOnly? to,
        string reason,
        UserId actor,
        DateTimeOffset now
    ) =>
        new()
        {
            Id = Guid.NewGuid(),
            StudentBranchPortfolioId = new StudentBranchPortfolioId(board),
            BranchId = branch,
            Type = type,
            ServicesAllowed = services,
            EffectiveFrom = from,
            EffectiveTo = to,
            Reason = reason,
            Status =
                from > DateOnly.FromDateTime(now.UtcDateTime)
                    ? StudentBranchAssignmentStatus.Planned
                    : StudentBranchAssignmentStatus.Active,
            CreatedByUserId = actor,
            CreatedAtUtc = now,
        };

    internal void End(DateOnly on, UserId actor, DateTimeOffset now)
    {
        EffectiveTo = on;
        Status = StudentBranchAssignmentStatus.Ended;
        EndedByUserId = actor;
        EndedAtUtc = now;
    }

    internal void ScheduleEnd(DateOnly on, UserId actor, DateTimeOffset now)
    {
        EffectiveTo = on;
        if (on <= DateOnly.FromDateTime(now.UtcDateTime))
        {
            Status = StudentBranchAssignmentStatus.Ended;
            EndedByUserId = actor;
            EndedAtUtc = now;
        }
    }
}

public sealed class PrimaryBranchChangeAnalysis
{
    private readonly List<BranchChangeImpact> impacts = [];

    private PrimaryBranchChangeAnalysis() { }

    public Guid Id { get; private set; }
    public StudentBranchPortfolioId StudentBranchPortfolioId { get; private set; }
    public BranchId? CurrentBranchId { get; private set; }
    public BranchId TargetBranchId { get; private set; }
    public DateTimeOffset AnalyzedAtUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public UserId AnalyzedByUserId { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }
    public UserId? AppliedByUserId { get; private set; }
    public IReadOnlyCollection<BranchChangeImpact> Impacts => impacts;

    internal static PrimaryBranchChangeAnalysis Create(
        Guid board,
        BranchId? current,
        BranchId target,
        IReadOnlyList<BranchChangeImpact> values,
        UserId actor,
        DateTimeOffset now
    )
    {
        var x = new PrimaryBranchChangeAnalysis
        {
            Id = Guid.NewGuid(),
            StudentBranchPortfolioId = new StudentBranchPortfolioId(board),
            CurrentBranchId = current,
            TargetBranchId = target,
            AnalyzedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(30),
            AnalyzedByUserId = actor,
        };
        x.impacts.AddRange(values.Select(v => v.Attach(x.Id)));
        return x;
    }

    internal bool CanApply(DateTimeOffset now) => AppliedAtUtc is null && ExpiresAtUtc >= now;

    internal void MarkApplied(UserId actor, DateTimeOffset now)
    {
        AppliedByUserId = actor;
        AppliedAtUtc = now;
    }
}

public sealed class BranchChangeImpact
{
    private BranchChangeImpact() { }

    public Guid Id { get; private set; }
    public Guid PrimaryBranchChangeAnalysisId { get; private set; }
    public BranchImpactType Type { get; private set; }
    public int AffectedCount { get; private set; }
    public string MessageKey { get; private set; } = string.Empty;
    public bool RequiresAction { get; private set; }

    public BranchChangeImpact(
        BranchImpactType type,
        int count,
        string messageKey,
        bool requiresAction
    )
    {
        Id = Guid.NewGuid();
        Type = type;
        AffectedCount = count;
        MessageKey = messageKey;
        RequiresAction = requiresAction;
    }

    internal BranchChangeImpact Attach(Guid analysis)
    {
        PrimaryBranchChangeAnalysisId = analysis;
        return this;
    }
}
