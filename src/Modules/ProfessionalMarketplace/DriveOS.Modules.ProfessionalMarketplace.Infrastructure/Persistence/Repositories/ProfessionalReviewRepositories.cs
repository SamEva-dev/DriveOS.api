using DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.ProfessionalMarketplace.Infrastructure.Persistence.Repositories;
internal sealed class ProfessionalReviewRepository(ProfessionalMarketplaceDbContext db):IProfessionalReviewRepository
{
 public Task<ProfessionalReview?> GetAsync(ProfessionalReviewId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalReviews.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalReviews.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
 public Task<bool> ExistsForEngagementAsync(ProfessionalEngagementId id,CancellationToken ct=default)=>db.ProfessionalReviews.AnyAsync(x=>x.EngagementId==id,ct);
 public async Task<IReadOnlyList<ProfessionalReview>> ListPublishedByProfileAsync(ProfessionalProfileId id,CancellationToken ct=default)=>await db.ProfessionalReviews.AsNoTracking().Where(x=>x.ProfessionalProfileId==id&&x.Status==ProfessionalReviewStatus.Published).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
 public async Task<IReadOnlyList<ProfessionalReview>> ListByProfileAsync(ProfessionalProfileId id,CancellationToken ct=default)=>await db.ProfessionalReviews.AsNoTracking().Where(x=>x.ProfessionalProfileId==id).OrderByDescending(x=>x.CreatedAtUtc).ToListAsync(ct);
 public void Add(ProfessionalReview x)=>db.ProfessionalReviews.Add(x);
}
internal sealed class ProfessionalReviewReportRepository(ProfessionalMarketplaceDbContext db):IProfessionalReviewReportRepository
{
 public Task<ProfessionalReviewReport?> GetAsync(ProfessionalReviewReportId id,bool tracking,CancellationToken ct=default)=>tracking?db.ProfessionalReviewReports.SingleOrDefaultAsync(x=>x.Id==id,ct):db.ProfessionalReviewReports.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
 public Task<bool> OpenReportExistsAsync(ProfessionalReviewId reviewId,UserId reporter,CancellationToken ct=default)=>db.ProfessionalReviewReports.AnyAsync(x=>x.ReviewId==reviewId&&x.ReportedByUserId==reporter&&x.Status==ProfessionalReviewReportStatus.Open,ct);
 public async Task<IReadOnlyList<ProfessionalReviewReport>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default)=>await (from report in db.ProfessionalReviewReports.AsNoTracking() join review in db.ProfessionalReviews.AsNoTracking() on report.ReviewId equals review.Id where review.ProfessionalProfileId==profileId orderby report.CreatedAtUtc descending select report).ToListAsync(ct);
 public void Add(ProfessionalReviewReport x)=>db.ProfessionalReviewReports.Add(x);
}
