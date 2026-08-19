using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;

public interface IPedagogicalReviewRepository
{
    Task<PedagogicalReview?> GetByIdAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken cancellationToken = default);
    Task<PedagogicalReview?> GetByIdForUpdateAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken cancellationToken = default);
    Task<bool> HasOpenReviewAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CancellationToken cancellationToken = default);
    Task AddAsync(PedagogicalReview review, CancellationToken cancellationToken = default);
}
