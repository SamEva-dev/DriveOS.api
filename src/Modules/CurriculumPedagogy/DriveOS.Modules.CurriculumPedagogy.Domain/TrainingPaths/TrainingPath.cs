using DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.TrainingPaths;

public sealed class TrainingPath : AggregateRoot<TrainingPathId>, IAuditableEntity
{
    private readonly List<TrainingPathMilestone> _milestones = [];

    private TrainingPath() { }

    private TrainingPath(
        TrainingPathId id,
        OrganizationId organizationId,
        PersonId studentId,
        CurriculumVersionId curriculumVersionId,
        TrainingMode trainingMode,
        DateOnly startDate,
        DateOnly? targetCompletionDate,
        decimal? estimatedPracticalHours)
        : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        CurriculumVersionId = curriculumVersionId;
        TrainingMode = trainingMode;
        StartDate = startDate;
        TargetCompletionDate = targetCompletionDate;
        EstimatedPracticalHours = estimatedPracticalHours;
        Status = TrainingPathStatus.Draft;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public CurriculumVersionId CurriculumVersionId { get; private set; }
    public TrainingMode TrainingMode { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? TargetCompletionDate { get; private set; }
    public decimal? EstimatedPracticalHours { get; private set; }
    public TrainingPathStatus Status { get; private set; }
    public IReadOnlyCollection<TrainingPathMilestone> Milestones => _milestones.AsReadOnly();

    public DateTimeOffset? ActivatedAtUtc { get; private set; }
    public UserId? ActivatedByUserId { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? SuspendedAtUtc { get; private set; }
    public string? SuspensionReason { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<TrainingPath> Create(
        TrainingPathId id,
        OrganizationId organizationId,
        PersonId studentId,
        CurriculumVersionId curriculumVersionId,
        TrainingMode trainingMode,
        DateOnly startDate,
        DateOnly? targetCompletionDate,
        decimal? estimatedPracticalHours)
    {
        if (id.IsEmpty)
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidIdentifier);
        if (organizationId.IsEmpty)
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidOrganization);
        if (studentId.IsEmpty)
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidStudent);
        if (curriculumVersionId.IsEmpty)
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidCurriculumVersion);
        if (!Enum.IsDefined(trainingMode))
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidTrainingMode);
        if (startDate == default)
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidStartDate);
        if (targetCompletionDate.HasValue && targetCompletionDate.Value < startDate)
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidTargetDate);
        if (estimatedPracticalHours.HasValue &&
            (estimatedPracticalHours.Value <= 0 || estimatedPracticalHours.Value > 1000))
        {
            return Result.Failure<TrainingPath>(TrainingPathErrors.InvalidEstimatedPracticalHours);
        }

        var trainingPath = new TrainingPath(
            id, organizationId, studentId, curriculumVersionId, trainingMode,
            startDate, targetCompletionDate, estimatedPracticalHours);

        trainingPath.RaiseDomainEvent(new TrainingPathCreatedDomainEvent(
            trainingPath.Id,
            trainingPath.OrganizationId,
            trainingPath.StudentId,
            trainingPath.CurriculumVersionId,
            trainingPath.TrainingMode));

        return Result.Success(trainingPath);
    }

    public Result UpdatePlan(
        TrainingMode trainingMode,
        DateOnly? targetCompletionDate,
        decimal? estimatedPracticalHours)
    {
        if (Status is not TrainingPathStatus.Draft and not TrainingPathStatus.ReadyForActivation)
            return Result.Failure(TrainingPathErrors.ModificationNotAllowed);
        if (!Enum.IsDefined(trainingMode))
            return Result.Failure(TrainingPathErrors.InvalidTrainingMode);
        if (targetCompletionDate.HasValue && targetCompletionDate.Value < StartDate)
            return Result.Failure(TrainingPathErrors.InvalidTargetDate);
        if (estimatedPracticalHours.HasValue &&
            (estimatedPracticalHours.Value <= 0 || estimatedPracticalHours.Value > 1000))
        {
            return Result.Failure(TrainingPathErrors.InvalidEstimatedPracticalHours);
        }

        TrainingMode = trainingMode;
        TargetCompletionDate = targetCompletionDate;
        EstimatedPracticalHours = estimatedPracticalHours;
        return Result.Success();
    }

    public Result<TrainingPathMilestone> AddMilestone(
        TrainingPathMilestoneId milestoneId,
        string code,
        string name,
        string? description,
        int order,
        DateOnly? targetDate)
    {
        if (Status is not TrainingPathStatus.Draft and not TrainingPathStatus.ReadyForActivation)
            return Result.Failure<TrainingPathMilestone>(TrainingPathErrors.ModificationNotAllowed);

        Result<TrainingPathMilestone> result = TrainingPathMilestone.Create(
            milestoneId, Id, code, name, description, order, targetDate);
        if (result.IsFailure)
            return result;

        TrainingPathMilestone milestone = result.Value;
        if (_milestones.Any(x => string.Equals(x.Code, milestone.Code, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<TrainingPathMilestone>(TrainingPathErrors.MilestoneCodeAlreadyExists);
        if (_milestones.Any(x => x.Order == milestone.Order))
            return Result.Failure<TrainingPathMilestone>(TrainingPathErrors.MilestoneOrderAlreadyExists);

        _milestones.Add(milestone);
        RaiseDomainEvent(new TrainingPathMilestoneAddedDomainEvent(Id, milestone.Id, milestone.Code, milestone.Order));
        return Result.Success(milestone);
    }

    public Result MarkReadyForActivation()
    {
        if (Status != TrainingPathStatus.Draft)
            return Result.Failure(TrainingPathErrors.MarkReadyNotAllowed);

        Status = TrainingPathStatus.ReadyForActivation;
        RaiseDomainEvent(new TrainingPathMarkedReadyDomainEvent(Id, OrganizationId, StudentId));
        return Result.Success();
    }

    public Result Activate(UserId actorUserId, DateTimeOffset occurredAtUtc)
    {
        if (Status != TrainingPathStatus.ReadyForActivation || actorUserId.IsEmpty || occurredAtUtc == default)
            return Result.Failure(TrainingPathErrors.ActivationNotAllowed);

        Status = TrainingPathStatus.Active;
        ActivatedByUserId = actorUserId;
        ActivatedAtUtc = occurredAtUtc.ToUniversalTime();
        SuspensionReason = null;
        SuspendedAtUtc = null;
        RaiseDomainEvent(new TrainingPathActivatedDomainEvent(
            Id, OrganizationId, StudentId, ActivatedAtUtc.Value, actorUserId));
        return Result.Success();
    }

    public Result Suspend(string reason, DateTimeOffset occurredAtUtc)
    {
        if (Status != TrainingPathStatus.Active || string.IsNullOrWhiteSpace(reason) || occurredAtUtc == default)
            return Result.Failure(TrainingPathErrors.SuspensionNotAllowed);

        string normalizedReason = reason.Trim();
        if (normalizedReason.Length > 500)
            return Result.Failure(TrainingPathErrors.SuspensionNotAllowed);

        Status = TrainingPathStatus.Suspended;
        SuspensionReason = normalizedReason;
        SuspendedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new TrainingPathSuspendedDomainEvent(Id, OrganizationId, StudentId, normalizedReason));
        return Result.Success();
    }

    public Result Reactivate()
    {
        if (Status != TrainingPathStatus.Suspended)
            return Result.Failure(TrainingPathErrors.ReactivationNotAllowed);

        Status = TrainingPathStatus.Active;
        SuspensionReason = null;
        SuspendedAtUtc = null;
        RaiseDomainEvent(new TrainingPathReactivatedDomainEvent(Id, OrganizationId, StudentId));
        return Result.Success();
    }

    public Result StartMilestone(TrainingPathMilestoneId milestoneId)
    {
        if (Status != TrainingPathStatus.Active)
            return Result.Failure(TrainingPathErrors.ModificationNotAllowed);

        TrainingPathMilestone? milestone = _milestones.FirstOrDefault(x => x.Id == milestoneId);
        if (milestone is null)
            return Result.Failure(TrainingPathErrors.MilestoneNotFound);

        Result result = milestone.Start();
        if (result.IsFailure)
            return result;

        RaiseDomainEvent(new TrainingPathMilestoneStartedDomainEvent(Id, milestone.Id));
        return Result.Success();
    }

    public Result CompleteMilestone(
        TrainingPathMilestoneId milestoneId,
        UserId actorUserId,
        DateTimeOffset occurredAtUtc)
    {
        if (Status != TrainingPathStatus.Active)
            return Result.Failure(TrainingPathErrors.ModificationNotAllowed);

        TrainingPathMilestone? milestone = _milestones.FirstOrDefault(x => x.Id == milestoneId);
        if (milestone is null)
            return Result.Failure(TrainingPathErrors.MilestoneNotFound);

        Result result = milestone.Complete(actorUserId, occurredAtUtc);
        if (result.IsFailure)
            return result;

        RaiseDomainEvent(new TrainingPathMilestoneCompletedDomainEvent(
            Id, milestone.Id, actorUserId, milestone.CompletedAtUtc!.Value));
        return Result.Success();
    }

    public Result CancelMilestone(TrainingPathMilestoneId milestoneId)
    {
        if (Status is not TrainingPathStatus.Draft and not TrainingPathStatus.ReadyForActivation and not TrainingPathStatus.Active)
            return Result.Failure(TrainingPathErrors.ModificationNotAllowed);

        TrainingPathMilestone? milestone = _milestones.FirstOrDefault(x => x.Id == milestoneId);
        if (milestone is null)
            return Result.Failure(TrainingPathErrors.MilestoneNotFound);

        Result result = milestone.Cancel();
        if (result.IsFailure)
            return result;

        RaiseDomainEvent(new TrainingPathMilestoneCancelledDomainEvent(Id, milestone.Id));
        return Result.Success();
    }

    public Result Complete(DateTimeOffset occurredAtUtc)
    {
        if (Status != TrainingPathStatus.Active || occurredAtUtc == default)
            return Result.Failure(TrainingPathErrors.CompletionNotAllowed);
        if (_milestones.Any(x => x.Status is TrainingPathMilestoneStatus.Planned or TrainingPathMilestoneStatus.InProgress))
            return Result.Failure(TrainingPathErrors.OpenMilestonesRemain);

        Status = TrainingPathStatus.Completed;
        CompletedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new TrainingPathCompletedDomainEvent(Id, OrganizationId, StudentId, CompletedAtUtc.Value));
        return Result.Success();
    }

    public Result Cancel(string reason, DateTimeOffset occurredAtUtc)
    {
        if (Status is TrainingPathStatus.Completed or TrainingPathStatus.Cancelled ||
            string.IsNullOrWhiteSpace(reason) || occurredAtUtc == default)
        {
            return Result.Failure(TrainingPathErrors.CancellationNotAllowed);
        }

        string normalizedReason = reason.Trim();
        if (normalizedReason.Length > 500)
            return Result.Failure(TrainingPathErrors.CancellationNotAllowed);

        Status = TrainingPathStatus.Cancelled;
        CancellationReason = normalizedReason;
        CancelledAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new TrainingPathCancelledDomainEvent(Id, OrganizationId, StudentId, normalizedReason));
        return Result.Success();
    }
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
