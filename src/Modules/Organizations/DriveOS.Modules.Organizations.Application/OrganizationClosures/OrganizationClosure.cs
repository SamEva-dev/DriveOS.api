using DriveOS.Modules.Organizations.Domain.OrganizationClosures.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Domain.OrganizationClosures;

public sealed class OrganizationClosure : AggregateRoot<OrganizationClosureId>, IAuditableEntity
{
    public const int MaximumDetailsLength = 2000;

    private OrganizationClosure() { }

    private OrganizationClosure(
        OrganizationClosureId id,
        OrganizationId organizationId,
        OrganizationClosureReasonCode reasonCode,
        string? reasonDetails,
        DateTimeOffset requestedEffectiveAtUtc,
        OrganizationDataDisposition dataDisposition,
        DateTimeOffset? retentionUntilUtc,
        UserId requestedByUserId
    )
        : base(id)
    {
        OrganizationId = organizationId;
        ReasonCode = reasonCode;
        ReasonDetails = reasonDetails;
        RequestedEffectiveAtUtc = requestedEffectiveAtUtc;
        DataDisposition = dataDisposition;
        RetentionUntilUtc = retentionUntilUtc;
        RequestedByUserId = requestedByUserId;
        Status = OrganizationClosureStatus.Draft;
        Revision = 1;
    }

    public OrganizationId OrganizationId { get; private set; }
    public OrganizationClosureReasonCode ReasonCode { get; private set; }
    public string? ReasonDetails { get; private set; }
    public DateTimeOffset RequestedEffectiveAtUtc { get; private set; }
    public OrganizationDataDisposition DataDisposition { get; private set; }
    public DateTimeOffset? RetentionUntilUtc { get; private set; }
    public OrganizationClosureStatus Status { get; private set; }
    public UserId RequestedByUserId { get; private set; }
    public UserId? ReviewedByUserId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public DateTimeOffset? ScheduledAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string? DecisionComment { get; private set; }
    public int Revision { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public bool IsOpen =>
        Status
            is OrganizationClosureStatus.Draft
                or OrganizationClosureStatus.UnderReview
                or OrganizationClosureStatus.Approved
                or OrganizationClosureStatus.Scheduled;

    public static Result<OrganizationClosure> Create(
        OrganizationClosureId id,
        OrganizationId organizationId,
        OrganizationClosureReasonCode reasonCode,
        string? reasonDetails,
        DateTimeOffset requestedEffectiveAtUtc,
        OrganizationDataDisposition dataDisposition,
        DateTimeOffset? retentionUntilUtc,
        UserId requestedByUserId,
        DateTimeOffset nowUtc
    )
    {
        if (id.IsEmpty)
            return Result.Failure<OrganizationClosure>(OrganizationClosureErrors.EmptyId);
        if (organizationId.IsEmpty)
            return Result.Failure<OrganizationClosure>(
                OrganizationClosureErrors.EmptyOrganizationId
            );
        if (!Enum.IsDefined(reasonCode) || !Enum.IsDefined(dataDisposition))
            return Result.Failure<OrganizationClosure>(OrganizationClosureErrors.InvalidReason);
        if (requestedByUserId.IsEmpty)
            return Result.Failure<OrganizationClosure>(OrganizationClosureErrors.InvalidReason);

        string? normalizedDetails = NormalizeDetails(reasonDetails);
        if (
            reasonCode == OrganizationClosureReasonCode.Other
            && string.IsNullOrWhiteSpace(normalizedDetails)
        )
            return Result.Failure<OrganizationClosure>(OrganizationClosureErrors.DetailsRequired);
        if (normalizedDetails?.Length > MaximumDetailsLength)
            return Result.Failure<OrganizationClosure>(OrganizationClosureErrors.DetailsTooLong);
        if (requestedEffectiveAtUtc < nowUtc)
            return Result.Failure<OrganizationClosure>(
                OrganizationClosureErrors.InvalidEffectiveDate
            );
        if (retentionUntilUtc.HasValue && retentionUntilUtc.Value < requestedEffectiveAtUtc)
            return Result.Failure<OrganizationClosure>(
                OrganizationClosureErrors.InvalidRetentionDate
            );
        if (
            dataDisposition == OrganizationDataDisposition.AnonymizeAfterRetention
            && !retentionUntilUtc.HasValue
        )
            return Result.Failure<OrganizationClosure>(
                OrganizationClosureErrors.InvalidRetentionDate
            );

        var closure = new OrganizationClosure(
            id,
            organizationId,
            reasonCode,
            normalizedDetails,
            requestedEffectiveAtUtc,
            dataDisposition,
            retentionUntilUtc,
            requestedByUserId
        );

        closure.RaiseDomainEvent(
            new OrganizationClosureCreatedDomainEvent(
                closure.Id,
                closure.OrganizationId,
                closure.ReasonCode,
                closure.RequestedEffectiveAtUtc
            )
        );

        return Result.Success(closure);
    }

    public Result UpdatePlan(
        OrganizationClosureReasonCode reasonCode,
        string? reasonDetails,
        DateTimeOffset requestedEffectiveAtUtc,
        OrganizationDataDisposition dataDisposition,
        DateTimeOffset? retentionUntilUtc,
        DateTimeOffset nowUtc
    )
    {
        if (Status != OrganizationClosureStatus.Draft)
            return Result.Failure(OrganizationClosureErrors.InvalidStatusTransition);

        Result<OrganizationClosure> validation = Create(
            OrganizationClosureId.New(),
            OrganizationId,
            reasonCode,
            reasonDetails,
            requestedEffectiveAtUtc,
            dataDisposition,
            retentionUntilUtc,
            RequestedByUserId,
            nowUtc
        );
        if (validation.IsFailure)
            return Result.Failure(validation.Error);

        ReasonCode = reasonCode;
        ReasonDetails = NormalizeDetails(reasonDetails);
        RequestedEffectiveAtUtc = requestedEffectiveAtUtc;
        DataDisposition = dataDisposition;
        RetentionUntilUtc = retentionUntilUtc;
        Revision++;
        return Result.Success();
    }

    public Result SubmitForReview(UserId actorUserId) =>
        ChangeStatus(
            OrganizationClosureStatus.Draft,
            OrganizationClosureStatus.UnderReview,
            actorUserId,
            null
        );

    public Result Approve(UserId actorUserId, string? comment, DateTimeOffset reviewedAtUtc)
    {
        Result result = ChangeStatus(
            OrganizationClosureStatus.UnderReview,
            OrganizationClosureStatus.Approved,
            actorUserId,
            comment
        );
        if (result.IsFailure)
            return result;
        ReviewedByUserId = actorUserId;
        ReviewedAtUtc = reviewedAtUtc;
        DecisionComment = NormalizeDetails(comment);
        return Result.Success();
    }

    public Result Reject(UserId actorUserId, string? comment, DateTimeOffset reviewedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure(OrganizationClosureErrors.DetailsRequired);
        Result result = ChangeStatus(
            OrganizationClosureStatus.UnderReview,
            OrganizationClosureStatus.Rejected,
            actorUserId,
            comment
        );
        if (result.IsFailure)
            return result;
        ReviewedByUserId = actorUserId;
        ReviewedAtUtc = reviewedAtUtc;
        DecisionComment = NormalizeDetails(comment);
        return Result.Success();
    }

    public Result Schedule(UserId actorUserId, DateTimeOffset scheduledAtUtc)
    {
        Result result = ChangeStatus(
            OrganizationClosureStatus.Approved,
            OrganizationClosureStatus.Scheduled,
            actorUserId,
            null
        );
        if (result.IsFailure)
            return result;
        ScheduledAtUtc = scheduledAtUtc;
        return Result.Success();
    }

    public Result Complete(UserId actorUserId, DateTimeOffset completedAtUtc)
    {
        Result result = ChangeStatus(
            OrganizationClosureStatus.Scheduled,
            OrganizationClosureStatus.Completed,
            actorUserId,
            null
        );
        if (result.IsFailure)
            return result;
        CompletedAtUtc = completedAtUtc;
        return Result.Success();
    }

    public Result Cancel(UserId actorUserId, string? comment, DateTimeOffset cancelledAtUtc)
    {
        if (
            !IsOpen
            || Status == OrganizationClosureStatus.Scheduled
                && cancelledAtUtc >= RequestedEffectiveAtUtc
        )
            return Result.Failure(OrganizationClosureErrors.InvalidStatusTransition);
        OrganizationClosureStatus previous = Status;
        Status = OrganizationClosureStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        DecisionComment = NormalizeDetails(comment);
        Revision++;
        RaiseDomainEvent(
            new OrganizationClosureStatusChangedDomainEvent(
                Id,
                OrganizationId,
                previous,
                Status,
                actorUserId,
                DecisionComment
            )
        );
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset createdAtUtc, UserId? createdByUserId)
    {
        if (CreatedAtUtc != default)
            return;
        CreatedAtUtc = createdAtUtc;
        CreatedByUserId = createdByUserId;
    }

    public void SetModifiedAudit(DateTimeOffset modifiedAtUtc, UserId? modifiedByUserId)
    {
        LastModifiedAtUtc = modifiedAtUtc;
        LastModifiedByUserId = modifiedByUserId;
    }

    private Result ChangeStatus(
        OrganizationClosureStatus expected,
        OrganizationClosureStatus next,
        UserId actorUserId,
        string? comment
    )
    {
        if (Status != expected || actorUserId.IsEmpty)
            return Result.Failure(OrganizationClosureErrors.InvalidStatusTransition);
        OrganizationClosureStatus previous = Status;
        Status = next;
        Revision++;
        RaiseDomainEvent(
            new OrganizationClosureStatusChangedDomainEvent(
                Id,
                OrganizationId,
                previous,
                next,
                actorUserId,
                NormalizeDetails(comment)
            )
        );
        return Result.Success();
    }

    private static string? NormalizeDetails(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
