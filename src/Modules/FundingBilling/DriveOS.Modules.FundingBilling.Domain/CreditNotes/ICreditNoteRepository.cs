using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Domain.CreditNotes;
public interface ICreditNoteRepository
{
    Task<CreditNote?> GetByIdAsync(CreditNoteId id, CancellationToken cancellationToken=default);
    Task AddAsync(CreditNote creditNote, CancellationToken cancellationToken=default);
}
