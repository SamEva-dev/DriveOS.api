using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
public static class ProfessionalInvoiceErrors
{
    public static readonly Error NotFound=Error.NotFound("ProfessionalMarketplace.Invoices.NotFound","errors.professionalMarketplace.invoices.notFound");
    public static readonly Error InvalidIdentifier=Error.Validation("ProfessionalMarketplace.Invoices.InvalidIdentifier","errors.professionalMarketplace.invoices.invalidIdentifier");
    public static readonly Error ApprovedStatementRequired=Error.Conflict("ProfessionalMarketplace.Invoices.ApprovedStatementRequired","errors.professionalMarketplace.invoices.approvedStatementRequired");
    public static readonly Error NoApprovedAmount=Error.Conflict("ProfessionalMarketplace.Invoices.NoApprovedAmount","errors.professionalMarketplace.invoices.noApprovedAmount");
    public static readonly Error InvalidAmounts=Error.Validation("ProfessionalMarketplace.Invoices.InvalidAmounts","errors.professionalMarketplace.invoices.invalidAmounts");
    public static readonly Error InvalidCurrency=Error.Validation("ProfessionalMarketplace.Invoices.InvalidCurrency","errors.professionalMarketplace.invoices.invalidCurrency");
    public static readonly Error InvoiceNumberRequired=Error.Validation("ProfessionalMarketplace.Invoices.InvoiceNumberRequired","errors.professionalMarketplace.invoices.invoiceNumberRequired");
    public static readonly Error DuplicateStatement=Error.Conflict("ProfessionalMarketplace.Invoices.DuplicateStatement","errors.professionalMarketplace.invoices.duplicateStatement");
    public static readonly Error InvalidTransition=Error.Conflict("ProfessionalMarketplace.Invoices.InvalidTransition","errors.professionalMarketplace.invoices.invalidTransition");
    public static readonly Error ImmutableAfterValidation=Error.Conflict("ProfessionalMarketplace.Invoices.ImmutableAfterValidation","errors.professionalMarketplace.invoices.immutableAfterValidation");
    public static readonly Error ImmutableAfterRequest=Error.Conflict("ProfessionalMarketplace.Invoices.ImmutableAfterRequest","errors.professionalMarketplace.invoices.immutableAfterRequest");
    public static readonly Error InvalidFinanceReference=Error.Conflict("ProfessionalMarketplace.Invoices.InvalidFinanceReference","errors.professionalMarketplace.invoices.invalidFinanceReference");
}
