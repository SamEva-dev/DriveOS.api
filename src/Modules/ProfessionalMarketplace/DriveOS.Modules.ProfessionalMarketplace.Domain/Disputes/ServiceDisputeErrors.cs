using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;

public static class ServiceDisputeErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Disputes.NotFound","errors.professionalMarketplace.disputes.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Disputes.InvalidIdentifier","errors.professionalMarketplace.disputes.invalidIdentifier");
    public static readonly Error InvalidDescription=Error.Validation("ProfessionalMarketplace.Disputes.InvalidDescription","errors.professionalMarketplace.disputes.invalidDescription");
    public static readonly Error InvalidEvidence=Error.Validation("ProfessionalMarketplace.Disputes.InvalidEvidence","errors.professionalMarketplace.disputes.invalidEvidence");
    public static readonly Error InvalidMessage=Error.Validation("ProfessionalMarketplace.Disputes.InvalidMessage","errors.professionalMarketplace.disputes.invalidMessage");
    public static readonly Error DuplicateOpenDispute=Error.Conflict("ProfessionalMarketplace.Disputes.DuplicateOpenDispute","errors.professionalMarketplace.disputes.duplicateOpenDispute");
    public static readonly Error Closed=Error.Conflict("ProfessionalMarketplace.Disputes.Closed","errors.professionalMarketplace.disputes.closed");
    public static readonly Error ResolutionRequired=Error.Validation("ProfessionalMarketplace.Disputes.ResolutionRequired","errors.professionalMarketplace.disputes.resolutionRequired");
    public static readonly Error EscalationReasonRequired=Error.Validation("ProfessionalMarketplace.Disputes.EscalationReasonRequired","errors.professionalMarketplace.disputes.escalationReasonRequired");
}
