using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Reviews;
public static class ProfessionalReviewErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Reviews.NotFound","errors.professionalMarketplace.reviews.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Reviews.InvalidIdentifier","errors.professionalMarketplace.reviews.invalidIdentifier");
    public static readonly Error DuplicateReview=Error.Conflict("ProfessionalMarketplace.Reviews.DuplicateReview","errors.professionalMarketplace.reviews.duplicateReview");
    public static readonly Error CompletedCollaborationRequired=Error.Conflict("ProfessionalMarketplace.Reviews.CompletedCollaborationRequired","errors.professionalMarketplace.reviews.completedCollaborationRequired");
    public static readonly Error InvalidRatings=Error.Validation("ProfessionalMarketplace.Reviews.InvalidRatings","errors.professionalMarketplace.reviews.invalidRatings");
    public static readonly Error InvalidComment=Error.Validation("ProfessionalMarketplace.Reviews.InvalidComment","errors.professionalMarketplace.reviews.invalidComment");
    public static readonly Error ProfileAccessDenied=Error.Forbidden("ProfessionalMarketplace.Reviews.ProfileAccessDenied","errors.professionalMarketplace.reviews.profileAccessDenied");
    public static readonly Error OrganizationMismatch=Error.Forbidden("ProfessionalMarketplace.Reviews.OrganizationMismatch","errors.professionalMarketplace.reviews.organizationMismatch");
    public static readonly Error InvalidResponse=Error.Validation("ProfessionalMarketplace.Reviews.InvalidResponse","errors.professionalMarketplace.reviews.invalidResponse");
    public static readonly Error ReviewNotRespondable=Error.Conflict("ProfessionalMarketplace.Reviews.ReviewNotRespondable","errors.professionalMarketplace.reviews.reviewNotRespondable");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Reviews.InvalidTransition","errors.professionalMarketplace.reviews.invalidTransition");
    public static readonly Error ModerationReasonRequired=Error.Validation("ProfessionalMarketplace.Reviews.ModerationReasonRequired","errors.professionalMarketplace.reviews.moderationReasonRequired");
    public static readonly Error InvalidReport=Error.Validation("ProfessionalMarketplace.Reviews.InvalidReport","errors.professionalMarketplace.reviews.invalidReport");
    public static readonly Error DuplicateOpenReport=Error.Conflict("ProfessionalMarketplace.Reviews.DuplicateOpenReport","errors.professionalMarketplace.reviews.duplicateOpenReport");
    public static readonly Error ReportNotFound=Error.NotFound("ProfessionalMarketplace.Reviews.ReportNotFound","errors.professionalMarketplace.reviews.reportNotFound");
    public static readonly Error InvalidReportResolution=Error.Validation("ProfessionalMarketplace.Reviews.InvalidReportResolution","errors.professionalMarketplace.reviews.invalidReportResolution");
}
