using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Offers;

public static class CommercialOfferErrors
{
    public static readonly Error MandatoryLineRequired = Error.Validation(
        "Crm.Offers.MandatoryLineRequired", "errors.crm.offers.mandatoryLineRequired");
    public static readonly Error InvalidIdentifier = Error.Validation("Crm.Offers.Id.Invalid", "errors.crm.offers.id.invalid");
    public static readonly Error InvalidAmount = Error.Validation("Crm.Offers.Amount.Invalid", "errors.crm.offers.amount.invalid");
    public static readonly Error InvalidCurrency = Error.Validation("Crm.Offers.Currency.Invalid", "errors.crm.offers.currency.invalid");
    public static readonly Error InvalidValidity = Error.Validation("Crm.Offers.Validity.Invalid", "errors.crm.offers.validity.invalid");
    public static readonly Error InvalidVersion = Error.Validation("Crm.Offers.Version.Invalid", "errors.crm.offers.version.invalid");
    public static readonly Error InvalidTransition = Error.Conflict("Crm.Offers.Status.InvalidTransition", "errors.crm.offers.status.invalidTransition");
    public static readonly Error InvalidTraining = Error.Validation("Crm.Offers.Training.Invalid", "errors.crm.offers.training.invalid");
    public static readonly Error InvalidLine = Error.Validation("Crm.Offers.Line.Invalid", "errors.crm.offers.line.invalid");
    public static readonly Error ManualOverrideReasonRequired = Error.Validation("Crm.Offers.ManualOverride.ReasonRequired", "errors.crm.offers.manualOverride.reasonRequired");
    public static readonly Error AssessmentResultMustBeValidated = Error.Validation("Crm.Offers.AssessmentResult.NotValidated", "errors.crm.offers.assessmentResult.notValidated");
    public static readonly Error LeadMismatch = Error.Validation("Crm.Offers.AssessmentResult.LeadMismatch", "errors.crm.offers.assessmentResult.leadMismatch");
    public static readonly Error NotFound = Error.NotFound("Crm.Offers.NotFound", "errors.crm.offers.notFound");
    public static readonly Error RecipientRequired = Error.Validation("Crm.Offers.Send.RecipientRequired", "errors.crm.offers.send.recipientRequired");
    public static readonly Error InvalidRecipient = Error.Validation("Crm.Offers.Send.InvalidRecipient", "errors.crm.offers.send.invalidRecipient");
    public static readonly Error InvalidMessage = Error.Validation("Crm.Offers.Send.InvalidMessage", "errors.crm.offers.send.invalidMessage");
    public static readonly Error InvalidLanguage = Error.Validation("Crm.Offers.Send.InvalidLanguage", "errors.crm.offers.send.invalidLanguage");
    public static readonly Error SecureLinkRequired = Error.Validation("Crm.Offers.Send.SecureLinkRequired", "errors.crm.offers.send.secureLinkRequired");
    public static readonly Error SecureLinkExpired = Error.Validation("Crm.Offers.Send.SecureLinkExpired", "errors.crm.offers.send.secureLinkExpired");
    public static readonly Error SecureLinkAlreadyRevoked = Error.Conflict("Crm.Offers.Send.SecureLinkAlreadyRevoked", "errors.crm.offers.send.secureLinkAlreadyRevoked");
    public static readonly Error InvalidInteraction = Error.Validation("Crm.Offers.Interaction.Invalid", "errors.crm.offers.interaction.invalid");
    public static readonly Error FollowUpDateRequired = Error.Validation("Crm.Offers.FollowUp.DateRequired", "errors.crm.offers.followUp.dateRequired");
    public static readonly Error OfferAlreadyExpired = Error.Conflict("Crm.Offers.AlreadyExpired", "errors.crm.offers.alreadyExpired");
}
