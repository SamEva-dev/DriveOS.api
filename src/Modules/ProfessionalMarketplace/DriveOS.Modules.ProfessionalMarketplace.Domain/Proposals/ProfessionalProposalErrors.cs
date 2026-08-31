using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Proposals;
public static class ProfessionalProposalErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Proposals.NotFound","errors.professionalMarketplace.proposals.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Proposals.InvalidIdentifier","errors.professionalMarketplace.proposals.invalidIdentifier");
    public static readonly Error DuplicateOpenProposal=Error.Conflict("ProfessionalMarketplace.Proposals.DuplicateOpenProposal","errors.professionalMarketplace.proposals.duplicateOpenProposal");
    public static readonly Error ProfileNotEligible=Error.Conflict("ProfessionalMarketplace.Proposals.ProfileNotEligible","errors.professionalMarketplace.proposals.profileNotEligible");
    public static readonly Error InvalidContent=Error.Validation("ProfessionalMarketplace.Proposals.InvalidContent","errors.professionalMarketplace.proposals.invalidContent");
    public static readonly Error InvalidRequirements=Error.Validation("ProfessionalMarketplace.Proposals.InvalidRequirements","errors.professionalMarketplace.proposals.invalidRequirements");
    public static readonly Error InvalidRate=Error.Validation("ProfessionalMarketplace.Proposals.InvalidRate","errors.professionalMarketplace.proposals.invalidRate");
    public static readonly Error InvalidExpiration=Error.Validation("ProfessionalMarketplace.Proposals.InvalidExpiration","errors.professionalMarketplace.proposals.invalidExpiration");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Proposals.InvalidTransition","errors.professionalMarketplace.proposals.invalidTransition");
    public static readonly Error ProposalExpired=Error.Conflict("ProfessionalMarketplace.Proposals.ProposalExpired","errors.professionalMarketplace.proposals.proposalExpired");
    public static readonly Error CounterNotAllowed=Error.Conflict("ProfessionalMarketplace.Proposals.CounterNotAllowed","errors.professionalMarketplace.proposals.counterNotAllowed");
    public static readonly Error NotYetExpired=Error.Conflict("ProfessionalMarketplace.Proposals.NotYetExpired","errors.professionalMarketplace.proposals.notYetExpired");
}
