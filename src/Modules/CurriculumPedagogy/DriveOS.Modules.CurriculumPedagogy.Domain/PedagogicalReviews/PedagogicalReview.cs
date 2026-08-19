using DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews.Events;
using DriveOS.SharedKernel.Auditing;
using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;

public sealed class PedagogicalReview : AggregateRoot<PedagogicalReviewId>, IAuditableEntity
{
    private PedagogicalReview() { }

    private PedagogicalReview(PedagogicalReviewId id, OrganizationId organizationId, PersonId studentId, TrainingPathId trainingPathId, UserId reviewerId, string reason, DateTimeOffset requestedAtUtc) : base(id)
    {
        OrganizationId = organizationId;
        StudentId = studentId;
        TrainingPathId = trainingPathId;
        ReviewerId = reviewerId;
        Reason = reason;
        RequestedAtUtc = requestedAtUtc.ToUniversalTime();
        Status = PedagogicalReviewStatus.Requested;
    }

    public OrganizationId OrganizationId { get; private set; }
    public PersonId StudentId { get; private set; }
    public TrainingPathId TrainingPathId { get; private set; }
    public UserId ReviewerId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Findings { get; private set; }
    public string? Recommendations { get; private set; }
    public decimal? EstimatedRemainingPracticalHours { get; private set; }
    public PedagogicalReviewStatus Status { get; private set; }
    public DateTimeOffset RequestedAtUtc { get; private set; }
    public DateTimeOffset? StartedAtUtc { get; private set; }
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public DateTimeOffset? LastModifiedAtUtc { get; private set; }
    public UserId? LastModifiedByUserId { get; private set; }

    public static Result<PedagogicalReview> Request(PedagogicalReviewId id, OrganizationId organizationId, PersonId studentId, TrainingPathId trainingPathId, UserId reviewerId, string reason, DateTimeOffset requestedAtUtc)
    {
        if (id.IsEmpty) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidIdentifier);
        if (organizationId.IsEmpty) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidOrganization);
        if (studentId.IsEmpty) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidStudent);
        if (trainingPathId.IsEmpty) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidTrainingPath);
        if (reviewerId.IsEmpty) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidReviewer);
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidReason);
        if (requestedAtUtc == default) return Result.Failure<PedagogicalReview>(PedagogicalReviewErrors.InvalidReason);
        var review = new PedagogicalReview(id, organizationId, studentId, trainingPathId, reviewerId, reason.Trim(), requestedAtUtc);
        review.RaiseDomainEvent(new PedagogicalReviewRequestedDomainEvent(review.Id, organizationId, studentId, trainingPathId, reviewerId));
        return Result.Success(review);
    }

    public Result Start(DateTimeOffset occurredAtUtc)
    {
        if (Status != PedagogicalReviewStatus.Requested || occurredAtUtc == default) return Result.Failure(PedagogicalReviewErrors.StartNotAllowed);
        Status = PedagogicalReviewStatus.InProgress;
        StartedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PedagogicalReviewStartedDomainEvent(Id, TrainingPathId, ReviewerId));
        return Result.Success();
    }

    public Result Complete(string findings, string recommendations, decimal? estimatedRemainingPracticalHours, DateTimeOffset occurredAtUtc)
    {
        if (Status is not PedagogicalReviewStatus.Requested and not PedagogicalReviewStatus.InProgress || occurredAtUtc == default) return Result.Failure(PedagogicalReviewErrors.CompletionNotAllowed);
        if (string.IsNullOrWhiteSpace(findings) || findings.Trim().Length > 8000) return Result.Failure(PedagogicalReviewErrors.InvalidFindings);
        if (string.IsNullOrWhiteSpace(recommendations) || recommendations.Trim().Length > 8000) return Result.Failure(PedagogicalReviewErrors.InvalidRecommendations);
        if (estimatedRemainingPracticalHours.HasValue && (estimatedRemainingPracticalHours <= 0 || estimatedRemainingPracticalHours > 1000)) return Result.Failure(PedagogicalReviewErrors.InvalidEstimatedRemainingNeeds);
        Findings = findings.Trim(); Recommendations = recommendations.Trim(); EstimatedRemainingPracticalHours = estimatedRemainingPracticalHours;
        Status = PedagogicalReviewStatus.Completed; CompletedAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PedagogicalReviewCompletedDomainEvent(Id, OrganizationId, StudentId, TrainingPathId, ReviewerId, CompletedAtUtc.Value));
        return Result.Success();
    }

    public Result Cancel(string reason, DateTimeOffset occurredAtUtc)
    {
        if (Status is PedagogicalReviewStatus.Completed or PedagogicalReviewStatus.Cancelled || string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 1000 || occurredAtUtc == default) return Result.Failure(PedagogicalReviewErrors.CancellationNotAllowed);
        Status = PedagogicalReviewStatus.Cancelled; CancellationReason = reason.Trim(); CancelledAtUtc = occurredAtUtc.ToUniversalTime();
        RaiseDomainEvent(new PedagogicalReviewCancelledDomainEvent(Id, TrainingPathId, CancellationReason));
        return Result.Success();
    }

    public void SetCreatedAudit(DateTimeOffset at, UserId? by) { if (CreatedAtUtc != default) return; CreatedAtUtc = at.ToUniversalTime(); CreatedByUserId = by; }
    public void SetModifiedAudit(DateTimeOffset at, UserId? by) { LastModifiedAtUtc = at.ToUniversalTime(); LastModifiedByUserId = by; }
}
