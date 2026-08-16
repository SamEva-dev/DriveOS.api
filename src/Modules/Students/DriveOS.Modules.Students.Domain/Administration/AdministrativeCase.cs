using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Administration;

public sealed class AdministrativeCase : AggregateRoot<AdministrativeCaseId>
{
    private readonly List<AdministrativeRequirement> requirements = [];
    private readonly List<AdministrativeBlock> blocks = [];
    private readonly List<ComplianceException> exceptions = [];
    private readonly List<AdministrativeHistoryEntry> history = [];

    private AdministrativeCase() { }

    private AdministrativeCase(AdministrativeCaseId id, OrganizationId organizationId, PersonId studentId)
        : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        Status = AdministrativeStatus.ToComplete;
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<AdministrativeCaseId>(Id, StudentId, OrganizationId, "Created"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public AdministrativeStatus Status { get; private set; }
    public IReadOnlyCollection<AdministrativeRequirement> Requirements => requirements;
    public IReadOnlyCollection<AdministrativeBlock> Blocks => blocks;
    public IReadOnlyCollection<ComplianceException> Exceptions => exceptions;
    public IReadOnlyCollection<AdministrativeHistoryEntry> History => history;

    public static Result<AdministrativeCase> Create(
        OrganizationId organizationId,
        PersonId studentId
    )
    {
        if (organizationId.IsEmpty || studentId.IsEmpty)
            return Result.Failure<AdministrativeCase>(AdministrativeErrors.InvalidOwner);
        return Result.Success(new AdministrativeCase(AdministrativeCaseId.New(), organizationId, studentId));
    }

    public Result<Guid> UpsertRequirement(
        Guid? requirementId,
        string code,
        string labelKey,
        bool isBlocking,
        DateTimeOffset? dueAtUtc,
        string policySource,
        UserId actor,
        DateTimeOffset now
    )
    {
        string normalizedCode = code?.Trim() ?? string.Empty;
        if (
            normalizedCode.Length is < 2 or > 80
            || string.IsNullOrWhiteSpace(labelKey)
            || labelKey.Length > 200
            || string.IsNullOrWhiteSpace(policySource)
            || policySource.Length > 100
        )
            return Result.Failure<Guid>(AdministrativeErrors.InvalidRequirement);
        AdministrativeRequirement? item = requirementId.HasValue
            ? requirements.SingleOrDefault(x => x.Id == requirementId.Value)
            : requirements.SingleOrDefault(x => x.Code == normalizedCode);
        if (requirementId.HasValue && item is null)
            return Result.Failure<Guid>(AdministrativeErrors.RequirementNotFound);
        if (item is null)
        {
            item = new AdministrativeRequirement(
                Guid.NewGuid(),
                Id,
                normalizedCode,
                labelKey.Trim(),
                isBlocking,
                dueAtUtc,
                policySource.Trim()
            );
            requirements.Add(item);
        }
        else
            item.Configure(
                normalizedCode,
                labelKey.Trim(),
                isBlocking,
                dueAtUtc,
                policySource.Trim()
            );
        AddHistory("RequirementConfigured", item.Id, actor, now, normalizedCode);
        Recalculate();
        return Result.Success(item.Id);
    }

    public Result DecideRequirement(
        Guid id,
        AdministrativeRequirementStatus status,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        AdministrativeRequirement? item = requirements.SingleOrDefault(x => x.Id == id);
        if (item is null)
            return Result.Failure(AdministrativeErrors.RequirementNotFound);
        if (
            status
            is not (
                AdministrativeRequirementStatus.Submitted
                or AdministrativeRequirementStatus.Validated
                or AdministrativeRequirementStatus.Rejected
                or AdministrativeRequirementStatus.Expired
            )
        )
            return Result.Failure(AdministrativeErrors.InvalidRequirementStatus);
        if (status == AdministrativeRequirementStatus.Rejected && string.IsNullOrWhiteSpace(reason))
            return Result.Failure(AdministrativeErrors.DecisionReasonRequired);
        item.ChangeStatus(status, NormalizeReason(reason), actor, now);
        AddHistory($"Requirement{status}", id, actor, now, NormalizeReason(reason));
        Recalculate();
        return Result.Success();
    }

    public Result<Guid> AddBlock(string code, string reason, UserId actor, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(reason))
            return Result.Failure<Guid>(AdministrativeErrors.BlockReasonRequired);
        var block = new AdministrativeBlock(
            Guid.NewGuid(),
            Id,
            code.Trim(),
            reason.Trim(),
            actor,
            now
        );
        blocks.Add(block);
        AddHistory("BlockApplied", block.Id, actor, now, reason.Trim());
        Recalculate();
        return Result.Success(block.Id);
    }

    public Result ReleaseBlock(Guid id, string reason, UserId actor, DateTimeOffset now)
    {
        AdministrativeBlock? block = blocks.SingleOrDefault(x =>
            x.Id == id && x.ReleasedAtUtc == null
        );
        if (block is null)
            return Result.Failure(AdministrativeErrors.BlockNotFound);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(AdministrativeErrors.DecisionReasonRequired);
        block.Release(reason.Trim(), actor, now);
        AddHistory("BlockReleased", id, actor, now, reason.Trim());
        Recalculate();
        return Result.Success();
    }

    public Result<Guid> RequestException(
        Guid requirementId,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (requirements.All(x => x.Id != requirementId))
            return Result.Failure<Guid>(AdministrativeErrors.RequirementNotFound);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 10)
            return Result.Failure<Guid>(AdministrativeErrors.ExceptionReasonRequired);
        var item = new ComplianceException(
            Guid.NewGuid(),
            Id,
            requirementId,
            reason.Trim(),
            actor,
            now
        );
        exceptions.Add(item);
        AddHistory("ExceptionRequested", item.Id, actor, now, reason.Trim());
        return Result.Success(item.Id);
    }

    public Result DecideException(
        Guid exceptionId,
        bool approve,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        ComplianceException? item = exceptions.SingleOrDefault(x => x.Id == exceptionId);
        if (item is null)
            return Result.Failure(AdministrativeErrors.ExceptionNotFound);
        if (item.Status != ComplianceExceptionStatus.Requested)
            return Result.Failure(AdministrativeErrors.ExceptionAlreadyDecided);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(AdministrativeErrors.DecisionReasonRequired);
        item.Decide(approve, reason.Trim(), actor, now);
        if (approve)
            requirements.Single(x => x.Id == item.RequirementId).Waive(actor, now);
        AddHistory(
            approve ? "ExceptionApproved" : "ExceptionRejected",
            item.Id,
            actor,
            now,
            reason.Trim()
        );
        Recalculate();
        return Result.Success();
    }

    private void Recalculate()
    {
        if (blocks.Any(x => x.ReleasedAtUtc == null))
        {
            Status = AdministrativeStatus.Blocked;
            return;
        }
        if (
            requirements.Count > 0
            && requirements.All(x =>
                x.Status
                    is AdministrativeRequirementStatus.Validated
                        or AdministrativeRequirementStatus.Waived
            )
        )
        {
            Status = AdministrativeStatus.Compliant;
            return;
        }
        Status = requirements.Any(x => x.Status == AdministrativeRequirementStatus.Submitted)
            ? AdministrativeStatus.UnderReview
            : AdministrativeStatus.ToComplete;
    }

    private void AddHistory(
        string action,
        Guid subjectId,
        UserId actor,
        DateTimeOffset now,
        string detail
    ) =>
        history.Add(
            new AdministrativeHistoryEntry(
                Guid.NewGuid(),
                Id,
                action,
                subjectId,
                actor,
                now,
                detail
            )
        );

    private static string NormalizeReason(string? value) => value?.Trim() ?? string.Empty;
}

public sealed class AdministrativeRequirement
{
    private AdministrativeRequirement() { }

    internal AdministrativeRequirement(
        Guid id,
        Guid administrativeCaseId,
        string code,
        string labelKey,
        bool isBlocking,
        DateTimeOffset? dueAtUtc,
        string policySource
    )
    {
        Id = id;
        AdministrativeCaseId = new AdministrativeCaseId(administrativeCaseId);
        Code = code;
        LabelKey = labelKey;
        IsBlocking = isBlocking;
        DueAtUtc = dueAtUtc;
        PolicySource = policySource;
        Status = AdministrativeRequirementStatus.Missing;
    }

    public Guid Id { get; private set; }
    public AdministrativeCaseId AdministrativeCaseId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string LabelKey { get; private set; } = string.Empty;
    public bool IsBlocking { get; private set; }
    public DateTimeOffset? DueAtUtc { get; private set; }
    public string PolicySource { get; private set; } = string.Empty;
    public AdministrativeRequirementStatus Status { get; private set; }
    public string? DecisionReason { get; private set; }
    public UserId? DecidedByUserId { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }

    internal void Configure(
        string code,
        string label,
        bool blocking,
        DateTimeOffset? due,
        string source
    )
    {
        Code = code;
        LabelKey = label;
        IsBlocking = blocking;
        DueAtUtc = due;
        PolicySource = source;
    }

    internal void ChangeStatus(
        AdministrativeRequirementStatus status,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        Status = status;
        DecisionReason = reason;
        DecidedByUserId = actor;
        DecidedAtUtc = now;
    }

    internal void Waive(UserId actor, DateTimeOffset now) =>
        ChangeStatus(AdministrativeRequirementStatus.Waived, "Approved exception", actor, now);
}

public sealed class AdministrativeBlock
{
    private AdministrativeBlock() { }

    internal AdministrativeBlock(
        Guid id,
        Guid caseId,
        string code,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        Id = id;
        AdministrativeCaseId = new AdministrativeCaseId(caseId);
        Code = code;
        Reason = reason;
        AppliedByUserId = actor;
        AppliedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public AdministrativeCaseId AdministrativeCaseId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public UserId AppliedByUserId { get; private set; }
    public DateTimeOffset AppliedAtUtc { get; private set; }
    public string? ReleaseReason { get; private set; }
    public UserId? ReleasedByUserId { get; private set; }
    public DateTimeOffset? ReleasedAtUtc { get; private set; }

    internal void Release(string reason, UserId actor, DateTimeOffset now)
    {
        ReleaseReason = reason;
        ReleasedByUserId = actor;
        ReleasedAtUtc = now;
    }
}

public sealed class ComplianceException
{
    private ComplianceException() { }

    internal ComplianceException(
        Guid id,
        Guid caseId,
        Guid requirementId,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
    {
        Id = id;
        AdministrativeCaseId = new AdministrativeCaseId(caseId);
        RequirementId = requirementId;
        RequestReason = reason;
        RequestedByUserId = actor;
        RequestedAtUtc = now;
        Status = ComplianceExceptionStatus.Requested;
    }

    public Guid Id { get; private set; }
    public AdministrativeCaseId AdministrativeCaseId { get; private set; }
    public Guid RequirementId { get; private set; }
    public string RequestReason { get; private set; } = string.Empty;
    public UserId RequestedByUserId { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public ComplianceExceptionStatus Status { get; private set; }
    public string? DecisionReason { get; private set; }
    public UserId? DecidedByUserId { get; private set; }
    public DateTimeOffset? DecidedAtUtc { get; private set; }

    internal void Decide(bool approve, string reason, UserId actor, DateTimeOffset now)
    {
        Status = approve ? ComplianceExceptionStatus.Approved : ComplianceExceptionStatus.Rejected;
        DecisionReason = reason;
        DecidedByUserId = actor;
        DecidedAtUtc = now;
    }
}

public sealed class AdministrativeHistoryEntry
{
    private AdministrativeHistoryEntry() { }

    internal AdministrativeHistoryEntry(
        Guid id,
        Guid caseId,
        string action,
        Guid subjectId,
        UserId actor,
        DateTimeOffset now,
        string detail
    )
    {
        Id = id;
        AdministrativeCaseId = new AdministrativeCaseId(caseId);
        Action = action;
        SubjectId = subjectId;
        ActorUserId = actor;
        OccurredAtUtc = now;
        Detail = detail;
    }

    public Guid Id { get; private set; }
    public AdministrativeCaseId AdministrativeCaseId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public Guid SubjectId { get; private set; }
    public UserId ActorUserId { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string Detail { get; private set; } = string.Empty;
}
