using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.ProfessionalMarketplace.Domain.Invoices;
public interface IProfessionalInvoiceRepository
{
    Task<ProfessionalInvoice?> GetAsync(ProfessionalInvoiceId id,bool tracking,CancellationToken ct=default);
    Task<bool> ExistsForStatementAsync(ServiceStatementId statementId,CancellationToken ct=default);
    Task<IReadOnlyList<ProfessionalInvoice>> ListByEngagementAsync(ProfessionalEngagementId engagementId,CancellationToken ct=default);
    Task<ProfessionalInvoice?> GetEarliestPaidAsync(
        ProfessionalEngagementId engagementId,CancellationToken ct=default);
    void Add(ProfessionalInvoice invoice);
}
