using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.FundingBilling.Infrastructure.Persistence.Repositories;
internal sealed class CreditNoteRepository(FundingBillingDbContext db):ICreditNoteRepository
{
 public Task<CreditNote?> GetByIdAsync(CreditNoteId id,CancellationToken cancellationToken=default)=>db.CreditNotes.Include(x=>x.Lines).SingleOrDefaultAsync(x=>x.Id==id,cancellationToken);
 public async Task AddAsync(CreditNote creditNote,CancellationToken cancellationToken=default)=>await db.CreditNotes.AddAsync(creditNote,cancellationToken);
}
