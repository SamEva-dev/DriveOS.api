using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceStatements;
public static class ServiceStatementErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.ServiceStatements.NotFound","errors.professionalMarketplace.serviceStatements.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.ServiceStatements.InvalidIdentifier","errors.professionalMarketplace.serviceStatements.invalidIdentifier");
    public static readonly Error InvalidPeriod=Error.Validation("ProfessionalMarketplace.ServiceStatements.InvalidPeriod","errors.professionalMarketplace.serviceStatements.invalidPeriod");
    public static readonly Error NoEntries=Error.Validation("ProfessionalMarketplace.ServiceStatements.NoEntries","errors.professionalMarketplace.serviceStatements.noEntries");
    public static readonly Error UnsubmittedEntries=Error.Conflict("ProfessionalMarketplace.ServiceStatements.UnsubmittedEntries","errors.professionalMarketplace.serviceStatements.unsubmittedEntries");
    public static readonly Error MixedCurrencies=Error.Validation("ProfessionalMarketplace.ServiceStatements.MixedCurrencies","errors.professionalMarketplace.serviceStatements.mixedCurrencies");
    public static readonly Error DuplicatePeriod=Error.Conflict("ProfessionalMarketplace.ServiceStatements.DuplicatePeriod","errors.professionalMarketplace.serviceStatements.duplicatePeriod");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.ServiceStatements.InvalidTransition","errors.professionalMarketplace.serviceStatements.invalidTransition");
    public static readonly Error ReasonRequired=Error.Validation("ProfessionalMarketplace.ServiceStatements.ReasonRequired","errors.professionalMarketplace.serviceStatements.reasonRequired");
    public static readonly Error InvoiceNotAllowed=Error.Conflict("ProfessionalMarketplace.ServiceStatements.InvoiceNotAllowed","errors.professionalMarketplace.serviceStatements.invoiceNotAllowed");
}
