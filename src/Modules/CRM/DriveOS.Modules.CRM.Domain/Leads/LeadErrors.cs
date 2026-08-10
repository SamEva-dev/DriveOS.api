using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CRM.Domain.Leads;

public static class LeadErrors
{
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
}
