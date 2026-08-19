using DriveOS.Modules.CurriculumPedagogy.Application.PedagogicalReviews;
using DriveOS.Modules.CurriculumPedagogy.Domain.PedagogicalReviews;
using DriveOS.Modules.CurriculumPedagogy.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.CurriculumPedagogy.Infrastructure.Read;

internal sealed class PedagogicalReviewReadService(CurriculumPedagogyDbContext db) : IPedagogicalReviewReadService
{
    public async Task<IReadOnlyCollection<PedagogicalReviewResponse>> ListForTrainingPathAsync(OrganizationId organizationId, TrainingPathId trainingPathId, CancellationToken cancellationToken = default) =>
        await db.PedagogicalReviews.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.TrainingPathId == trainingPathId).OrderByDescending(x => x.RequestedAtUtc).Select(Project()).ToListAsync(cancellationToken);
    public Task<PedagogicalReviewResponse?> GetAsync(OrganizationId organizationId, PedagogicalReviewId reviewId, CancellationToken cancellationToken = default) =>
        db.PedagogicalReviews.AsNoTracking().Where(x => x.OrganizationId == organizationId && x.Id == reviewId).Select(Project()).SingleOrDefaultAsync(cancellationToken);
    private static System.Linq.Expressions.Expression<Func<PedagogicalReview, PedagogicalReviewResponse>> Project() => x => new(x.Id.Value, x.StudentId.Value, x.TrainingPathId.Value, x.ReviewerId.Value, x.Reason, x.Status.ToString(), x.Findings, x.Recommendations, x.EstimatedRemainingPracticalHours, x.RequestedAtUtc, x.StartedAtUtc, x.CompletedAtUtc, x.CancelledAtUtc, x.CancellationReason);
}
