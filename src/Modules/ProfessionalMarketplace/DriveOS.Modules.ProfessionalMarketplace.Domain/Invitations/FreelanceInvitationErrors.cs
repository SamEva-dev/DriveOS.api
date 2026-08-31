using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Invitations;
public static class FreelanceInvitationErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Invitations.NotFound","errors.professionalMarketplace.invitations.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Invitations.InvalidIdentifier","errors.professionalMarketplace.invitations.invalidIdentifier");
    public static readonly Error RecipientRequired=Error.Validation("ProfessionalMarketplace.Invitations.RecipientRequired","errors.professionalMarketplace.invitations.recipientRequired");
    public static readonly Error InvalidExpiration=Error.Validation("ProfessionalMarketplace.Invitations.InvalidExpiration","errors.professionalMarketplace.invitations.invalidExpiration");
    public static readonly Error InvalidToken=Error.Validation("ProfessionalMarketplace.Invitations.InvalidToken","errors.professionalMarketplace.invitations.invalidToken");
    public static readonly Error Expired=Error.Conflict("ProfessionalMarketplace.Invitations.Expired","errors.professionalMarketplace.invitations.expired");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Invitations.InvalidTransition","errors.professionalMarketplace.invitations.invalidTransition");
    public static readonly Error AuthenticationRequired=Error.Conflict("ProfessionalMarketplace.Invitations.AuthenticationRequired","errors.professionalMarketplace.invitations.authenticationRequired");
    public static readonly Error InvitedUserMismatch=Error.Conflict("ProfessionalMarketplace.Invitations.InvitedUserMismatch","errors.professionalMarketplace.invitations.invitedUserMismatch");
}
