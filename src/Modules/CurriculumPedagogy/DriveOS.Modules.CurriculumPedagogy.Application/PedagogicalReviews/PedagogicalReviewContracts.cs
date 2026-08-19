using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CurriculumPedagogy.Application.PedagogicalReviews;

public sealed record RequestPedagogicalReviewCommand(OrganizationId OrganizationId, TrainingPathId TrainingPathId, UserId ReviewerId, string Reason, UserId ActorUserId) : ICommand<PedagogicalReviewId>;
public sealed record StartPedagogicalReviewCommand(OrganizationId OrganizationId, PedagogicalReviewId ReviewId, UserId ActorUserId) : ICommand;
public sealed record CompletePedagogicalReviewCommand(OrganizationId OrganizationId, PedagogicalReviewId ReviewId, string Findings, string Recommendations, decimal? EstimatedRemainingPracticalHours, UserId ActorUserId) : ICommand;
public sealed record CancelPedagogicalReviewCommand(OrganizationId OrganizationId, PedagogicalReviewId ReviewId, string Reason, UserId ActorUserId) : ICommand;

public static class PedagogicalReviewApplicationErrors
{
    public static readonly Error TrainingPathNotFound = Error.NotFound("CurriculumPedagogy.PedagogicalReview.TrainingPath.NotFound", "errors.curriculumPedagogy.pedagogicalReview.trainingPath.notFound");
    public static readonly Error TrainingPathNotEligible = Error.Conflict("CurriculumPedagogy.PedagogicalReview.TrainingPath.NotEligible", "errors.curriculumPedagogy.pedagogicalReview.trainingPath.notEligible");
    public static readonly Error OpenReviewAlreadyExists = Error.Conflict("CurriculumPedagogy.PedagogicalReview.OpenReview.AlreadyExists", "errors.curriculumPedagogy.pedagogicalReview.openReview.alreadyExists");
    public static readonly Error ReviewNotFound = Error.NotFound("CurriculumPedagogy.PedagogicalReview.NotFound", "errors.curriculumPedagogy.pedagogicalReview.notFound");
}

public sealed record PedagogicalReviewResponse(Guid Id, Guid StudentId, Guid TrainingPathId, Guid ReviewerId, string Reason, string Status, string? Findings, string? Recommendations, decimal? EstimatedRemainingPracticalHours, DateTimeOffset RequestedAtUtc, DateTimeOffset? StartedAtUtc, DateTimeOffset? CompletedAtUtc, DateTimeOffset? CancelledAtUtc, string? CancellationReason);

public interface IPedagogicalReviewReadService
{
    Task<IReadOnlyCollection<PedagogicalReviewResponse>> ListForTrainingPathAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CancellationToken cancellationToken = default);
    Task<PedagogicalReviewResponse?> GetAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken cancellationToken = default);
}
