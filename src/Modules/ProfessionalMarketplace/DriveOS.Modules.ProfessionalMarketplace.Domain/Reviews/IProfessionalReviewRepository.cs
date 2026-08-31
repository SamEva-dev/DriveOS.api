using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
public interface IProfessionalReviewRepository
{
    Task<ProfessionalReview?> GetAsync(ProfessionalReviewId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsForEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalReview>> ListPublishedByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalReview>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ProfessionalReview review);
}
public interface IProfessionalReviewReportRepository
{
    Task<ProfessionalReviewReport?> GetAsync(ProfessionalReviewReportId id,bool tracking,CancellationToken ct=default);
    Task<bool> OpenReportExistsAsync(ProfessionalReviewId reviewId,UserId reporter,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalReviewReport>> ListByProfileAsync(ProfessionalProfileId profileId,CancellationToken ct=default);
    void Add(ProfessionalReviewReport report);
}
