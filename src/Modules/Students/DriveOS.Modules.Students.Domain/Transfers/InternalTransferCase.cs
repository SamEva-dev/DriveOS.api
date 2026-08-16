using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.Students.Domain.Events;

namespace DriveOS.Modules.Students.Domain.Transfers;

public sealed class InternalTransferCase : AggregateRoot<InternalTransferCaseId>
{
    private readonly List<InternalTransferImpact> impacts = [];

    private InternalTransferCase() { }

    private InternalTransferCase(
        InternalTransferCaseId id,
        OrganizationId org,
        PersonId student,
        BranchId source,
        BranchId target,
        InternalTransferMode mode,
        InternalTransferElement elements,
        DateOnly effectiveOn,
        DateOnly? temporaryUntil,
        string reason,
        UserId actor,
        DateTimeOffset now
    )
        : base(id)
    {
        OrganizationId = org;
        StudentId = student;
        SourceBranchId = source;
        TargetBranchId = target;
        Mode = mode;
        Elements = elements;
        EffectiveOn = effectiveOn;
        TemporaryUntil = temporaryUntil;
        Reason = reason;
        Status = InternalTransferStatus.Analyzed;
        AnalyzedByUserId = actor;
        AnalyzedAtUtc = now;
        AnalysisExpiresAtUtc = now.AddMinutes(30);
        RaiseDomainEvent(new StudentAggregateChangedDomainEvent<InternalTransferCaseId>(Id, StudentId, OrganizationId, "Analyzed"));
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public BranchId SourceBranchId { get; private set; }
    public BranchId TargetBranchId { get; private set; }
    public InternalTransferMode Mode { get; private set; }
    public InternalTransferElement Elements { get; private set; }
    public DateOnly EffectiveOn { get; private set; }
    public DateOnly? TemporaryUntil { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public InternalTransferStatus Status { get; private set; }
    public DateTimeOffset AnalyzedAtUtc { get; private set; }
    public DateTimeOffset AnalysisExpiresAtUtc { get; private set; }
    public UserId AnalyzedByUserId { get; private set; }
    public DateTimeOffset? ValidatedAtUtc { get; private set; }
    public UserId? ValidatedByUserId { get; private set; }
    public IReadOnlyCollection<InternalTransferImpact> Impacts => impacts;

    public static Result<InternalTransferCase> Create(
        OrganizationId org,
        PersonId student,
        BranchId source,
        BranchId target,
        InternalTransferMode mode,
        InternalTransferElement elements,
        DateOnly? requestedDate,
        DateOnly? temporaryUntil,
        string reason,
        IReadOnlyList<InternalTransferImpactSeed> impactSeeds,
        UserId actor,
        DateTimeOffset now
    )
    {
        if (org.IsEmpty || student.IsEmpty || source.IsEmpty || target.IsEmpty)
            return Result.Failure<InternalTransferCase>(InternalTransferErrors.InvalidOwner);
        if (source == target)
            return Result.Failure<InternalTransferCase>(InternalTransferErrors.SameBranch);
        if (elements == InternalTransferElement.None)
            return Result.Failure<InternalTransferCase>(InternalTransferErrors.InvalidRequest);
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure<InternalTransferCase>(InternalTransferErrors.ReasonRequired);
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        if (mode != InternalTransferMode.Immediate && !requestedDate.HasValue)
            return Result.Failure<InternalTransferCase>(
                InternalTransferErrors.EffectiveDateRequired
            );
        var effective = mode == InternalTransferMode.Immediate ? today : requestedDate!.Value;
        if (
            effective < today
            || mode == InternalTransferMode.Temporary
                && (!temporaryUntil.HasValue || temporaryUntil.Value <= effective)
        )
            return Result.Failure<InternalTransferCase>(InternalTransferErrors.InvalidRequest);
        var result = new InternalTransferCase(
            InternalTransferCaseId.New(),
            org,
            student,
            source,
            target,
            mode,
            elements,
            effective,
            temporaryUntil,
            reason.Trim(),
            actor,
            now
        );
        result.impacts.AddRange(
            impactSeeds.Select(x => InternalTransferImpact.Create(new ExternalTransferCaseId(result.Id), x))
        );
        return Result.Success(result);
    }

    public Result Validate(UserId actor, DateTimeOffset now)
    {
        if (Status != InternalTransferStatus.Analyzed)
            return Result.Failure(InternalTransferErrors.AlreadyValidated);
        if (AnalysisExpiresAtUtc < now)
        {
            Status = InternalTransferStatus.Expired;
            return Result.Failure(InternalTransferErrors.AnalysisExpired);
        }
        if (impacts.Any(x => x.Status == InternalTransferImpactStatus.Blocked))
            return Result.Failure(InternalTransferErrors.BlockingImpact);
        ValidatedAtUtc = now;
        ValidatedByUserId = actor;
        Status =
            EffectiveOn <= DateOnly.FromDateTime(now.UtcDateTime)
                ? InternalTransferStatus.Applied
                : InternalTransferStatus.Scheduled;
        return Result.Success();
    }

    public void ApplyScheduled(DateOnly today)
    {
        if (Status == InternalTransferStatus.Scheduled && EffectiveOn <= today)
            Status = InternalTransferStatus.Applied;
    }

    public void RevertTemporary(DateOnly today)
    {
        if (
            Status == InternalTransferStatus.Applied
            && Mode == InternalTransferMode.Temporary
            && TemporaryUntil.HasValue
            && TemporaryUntil.Value < today
        )
            Status = InternalTransferStatus.Reverted;
    }
}

public sealed record InternalTransferImpactSeed(
    InternalTransferImpactType Type,
    int AffectedCount,
    InternalTransferImpactStatus Status,
    string MessageKey,
    bool RequiresAction
);

public sealed class InternalTransferImpact
{
    private InternalTransferImpact() { }

    public Guid Id { get; private set; }
    public InternalTransferCaseId InternalTransferCaseId { get; private set; }
    public InternalTransferImpactType Type { get; private set; }
    public int AffectedCount { get; private set; }
    public InternalTransferImpactStatus Status { get; private set; }
    public string MessageKey { get; private set; } = string.Empty;
    public bool RequiresAction { get; private set; }

    internal static InternalTransferImpact Create(ExternalTransferCaseId transfer, InternalTransferImpactSeed seed) =>
        new()
        {
            Id = Guid.NewGuid(),
            InternalTransferCaseId = new InternalTransferCaseId(transfer),
            Type = seed.Type,
            AffectedCount = seed.AffectedCount,
            Status = seed.Status,
            MessageKey = seed.MessageKey,
            RequiresAction = seed.RequiresAction,
        };
}
