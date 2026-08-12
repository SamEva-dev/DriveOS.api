using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public static class LeadErrors
{
    public static readonly Error EmptyAssignedAdvisorId = Error.Validation(
        "Crm.Leads.AssignedAdvisorId.Empty", "errors.crm.leads.assignedAdvisorId.empty");
    public static readonly Error EmptyId = Error.Validation(
        "Crm.Leads.Id.Empty",
        "errors.crm.leads.id.empty");

    public static readonly Error EmptyOrganizationId = Error.Validation(
        "Crm.Leads.OrganizationId.Empty",
        "errors.crm.leads.organizationId.empty");

    public static readonly Error CurrentTenantRequired = Error.Unauthorized(
        "Crm.Leads.CurrentTenant.Required",
        "errors.crm.leads.currentTenant.required");

    public static readonly Error CurrentUserRequired = Error.Unauthorized(
        "Crm.Leads.CurrentUser.Required",
        "errors.crm.leads.currentUser.required");

    public static readonly Error FirstNameRequired = Error.Validation(
        "Crm.Leads.FirstName.Required",
        "errors.crm.leads.firstName.required");

    public static readonly Error FirstNameTooLong = Error.Validation(
        "Crm.Leads.FirstName.TooLong",
        "errors.crm.leads.firstName.tooLong");

    public static readonly Error LastNameRequired = Error.Validation(
        "Crm.Leads.LastName.Required",
        "errors.crm.leads.lastName.required");

    public static readonly Error LastNameTooLong = Error.Validation(
        "Crm.Leads.LastName.TooLong",
        "errors.crm.leads.lastName.tooLong");

    public static readonly Error InvalidEmail = Error.Validation(
        "Crm.Leads.Email.Invalid",
        "errors.crm.leads.email.invalid");

    public static readonly Error PhoneTooLong = Error.Validation(
        "Crm.Leads.Phone.TooLong",
        "errors.crm.leads.phone.tooLong");

    public static readonly Error LicenseCategoryRequired = Error.Validation(
        "Crm.Leads.LicenseCategory.Required",
        "errors.crm.leads.licenseCategory.required");

    public static readonly Error LicenseCategoryTooLong = Error.Validation(
        "Crm.Leads.LicenseCategory.TooLong",
        "errors.crm.leads.licenseCategory.tooLong");

    public static readonly Error InvalidTransmissionPreference = Error.Validation(
        "Crm.Leads.Transmission.Invalid",
        "errors.crm.leads.transmission.invalid");

    public static readonly Error PreferredLocationTooLong = Error.Validation(
        "Crm.Leads.PreferredLocation.TooLong",
        "errors.crm.leads.preferredLocation.tooLong");

    public static readonly Error InvalidSourceType = Error.Validation(
        "Crm.Leads.Source.Invalid",
        "errors.crm.leads.source.invalid");

    public static readonly Error SourceDetailTooLong = Error.Validation(
        "Crm.Leads.Source.DetailTooLong",
        "errors.crm.leads.source.detailTooLong");

    public static readonly Error SourceDetailRequired = Error.Validation(
        "Crm.Leads.Source.DetailRequired",
        "errors.crm.leads.source.detailRequired");

    public static readonly Error NotFound = Error.NotFound(
        "Crm.Leads.NotFound",
        "errors.crm.leads.notFound");

    public static readonly Error InvalidStatus = Error.Validation(
        "Crm.Leads.Status.Invalid",
        "errors.crm.leads.status.invalid");

    public static readonly Error StatusAlreadyApplied = Error.Conflict(
        "Crm.Leads.Status.AlreadyApplied",
        "errors.crm.leads.status.alreadyApplied");

    public static readonly Error InvalidStatusTransition = Error.Conflict(
        "Crm.Leads.Status.InvalidTransition",
        "errors.crm.leads.status.invalidTransition");

    public static readonly Error LossReasonRequired = Error.Validation(
        "Crm.Leads.LossReason.Required",
        "errors.crm.leads.lossReason.required");

    public static readonly Error StatusReasonTooLong = Error.Validation(
        "Crm.Leads.StatusReason.TooLong",
        "errors.crm.leads.statusReason.tooLong");

    public static readonly Error QualificationNotAllowed = Error.Conflict(
        "Crm.Leads.Qualification.NotAllowed", "errors.crm.leads.qualification.notAllowed");
    public static readonly Error QualificationNeedInvalid = Error.Validation(
        "Crm.Leads.Qualification.Need.Invalid", "errors.crm.leads.qualification.need.invalid");
    public static readonly Error QualificationCategoryInvalid = Error.Validation(
        "Crm.Leads.Qualification.Category.Invalid", "errors.crm.leads.qualification.category.invalid");
    public static readonly Error QualificationAvailabilityInvalid = Error.Validation(
        "Crm.Leads.Qualification.Availability.Invalid", "errors.crm.leads.qualification.availability.invalid");
    public static readonly Error QualificationFinancingInvalid = Error.Validation(
        "Crm.Leads.Qualification.Financing.Invalid", "errors.crm.leads.qualification.financing.invalid");
    public static readonly Error QualificationNotesTooLong = Error.Validation(
        "Crm.Leads.Qualification.Notes.TooLong", "errors.crm.leads.qualification.notes.tooLong");
    public static readonly Error ConversionRequiresWonStatus = Error.Conflict(
        "Crm.Conversions.LeadMustBeWon", "errors.crm.conversions.leadMustBeWon");
    public static readonly Error ConversionRequiresQualification = Error.Conflict(
        "Crm.Conversions.QualificationRequired", "errors.crm.conversions.qualificationRequired");
    public static readonly Error InvalidConversionTarget = Error.Validation(
        "Crm.Conversions.Target.Invalid", "errors.crm.conversions.target.invalid");
    public static readonly Error AlreadyConverted = Error.Conflict(
        "Crm.Conversions.AlreadyConverted", "errors.crm.conversions.alreadyConverted");
    public static readonly Error ConversionAcceptedOfferRequired = Error.Conflict(
        "Crm.Conversions.AcceptedOffer.Required", "errors.crm.conversions.acceptedOffer.required");
    public static readonly Error ConversionPreconditionsIncomplete = Error.Validation(
        "Crm.Conversions.Preconditions.Incomplete", "errors.crm.conversions.preconditions.incomplete");
    public static readonly Error InvalidClosureDecision = Error.Validation(
        "Crm.Leads.Closure.Decision.Invalid", "errors.crm.leads.closure.decision.invalid");
    public static readonly Error InvalidClosureReason = Error.Validation(
        "Crm.Leads.Closure.Reason.Invalid", "errors.crm.leads.closure.reason.invalid");
    public static readonly Error ClosureCommentTooLong = Error.Validation(
        "Crm.Leads.Closure.Comment.TooLong", "errors.crm.leads.closure.comment.tooLong");
    public static readonly Error DormancyResponsibleRequired = Error.Validation(
        "Crm.Leads.Dormancy.Responsible.Required", "errors.crm.leads.dormancy.responsible.required");
    public static readonly Error ResumeDateMustBeFuture = Error.Validation(
        "Crm.Leads.Dormancy.ResumeDate.Future", "errors.crm.leads.dormancy.resumeDate.future");
    public static readonly Error CampaignCodeTooLong = Error.Validation(
        "Crm.Leads.Dormancy.CampaignCode.TooLong", "errors.crm.leads.dormancy.campaignCode.tooLong");
    public static readonly Error PartnerNameInvalid = Error.Validation(
        "Crm.Leads.Referral.Partner.Invalid", "errors.crm.leads.referral.partner.invalid");
    public static readonly Error SharedDataDescriptionInvalid = Error.Validation(
        "Crm.Leads.Referral.SharedData.Invalid", "errors.crm.leads.referral.sharedData.invalid");
    public static readonly Error ReferralConsentRequired = Error.Validation(
        "Crm.Leads.Referral.Consent.Required", "errors.crm.leads.referral.consent.required");
    public static readonly Error ReopenNotAllowed = Error.Conflict(
        "Crm.Leads.Reopen.NotAllowed", "errors.crm.leads.reopen.notAllowed");
}
