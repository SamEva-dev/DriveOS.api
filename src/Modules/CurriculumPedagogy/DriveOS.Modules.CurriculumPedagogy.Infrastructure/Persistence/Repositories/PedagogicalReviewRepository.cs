using DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence.Repositories;

internal sealed class PedagogicalReviewRepository(CurriculumPedagogyDbContext db) : IPedagogicalReviewRepository
{
    public Task<PedagogicalReview?> GetByIdAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken cancellationToken = default) => db.PedagogicalReviews.AsNoTracking().SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == reviewId, cancellationToken);
    public Task<PedagogicalReview?> GetByIdForUpdateAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken cancellationToken = default) => db.PedagogicalReviews.SingleOrDefaultAsync(x => x.OrganizationId == organizationId && x.Id == reviewId, cancellationToken);
    public Task<bool> HasOpenReviewAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CancellationToken cancellationToken = default) => db.PedagogicalReviews.AsNoTracking().AnyAsync(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId && (x.Status == PedagogicalReviewStatus.Requested || x.Status == PedagogicalReviewStatus.InProgress), cancellationToken);
    public async Task AddAsync(PedagogicalReview review, CancellationToken cancellationToken = default) => await db.PedagogicalReviews.AddAsync(review, cancellationToken);
}
