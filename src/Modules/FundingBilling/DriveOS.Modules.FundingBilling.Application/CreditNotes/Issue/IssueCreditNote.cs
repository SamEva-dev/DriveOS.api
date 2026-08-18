using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.CreditNotes.Issue;
public sealed record IssueCreditNoteCommand(OrganizationId OrganizationId,CreditNoteId CreditNoteId,DateOnly IssueDate,UserId ActorUserId):ICommand<IssueCreditNoteResponse>;
public sealed record IssueCreditNoteResponse(CreditNoteId CreditNoteId,string CreditNoteNumber,DateOnly IssueDate,decimal TotalAmount,string Currency);
public interface ICreditNoteNumberGenerator{Task<Result<string>> ReserveNextAsync(OrganizationId organizationId,CancellationToken cancellationToken=default);}
public static class IssueCreditNoteErrors{public static readonly Error NumberSequenceNotConfigured=Error.Conflict("FundingBilling.CreditNote.NumberSequence.NotConfigured","errors.fundingBilling.creditNote.numberSequence.notConfigured");}
internal sealed class IssueCreditNoteCommandHandler(ICreditNoteRepository notes,IInvoiceRepository invoices,IStudentBillingAccountRepository accounts,ICreditNoteNumberGenerator numbers,IFundingBillingUnitOfWork uow,IClock clock):ICommandHandler<IssueCreditNoteCommand,IssueCreditNoteResponse>
{
 public async Task<Result<IssueCreditNoteResponse>> Handle(IssueCreditNoteCommand c,CancellationToken ct){var note=await notes.GetByIdAsync(c.CreditNoteId,ct);if(note is null||note.OrganizationId!=c.OrganizationId)return Result.Failure<IssueCreditNoteResponse>(CreditNoteErrors.NotFound);var invoice=await invoices.GetByIdAsync(note.InvoiceId,ct);if(invoice is null||invoice.OrganizationId!=c.OrganizationId)return Result.Failure<IssueCreditNoteResponse>(CreditNoteErrors.InvoiceNotFound);var account=await accounts.GetByIdAsync(note.BillingAccountId,ct);if(account is null||account.OrganizationId!=c.OrganizationId)return Result.Failure<IssueCreditNoteResponse>(CreditNoteErrors.BillingAccountNotFound);var number=await numbers.ReserveNextAsync(c.OrganizationId,ct);if(number.IsFailure)return Result.Failure<IssueCreditNoteResponse>(number.Error);var now=clock.UtcNow;var issued=note.Issue(number.Value,c.IssueDate,invoice.CreditableAmount,c.ActorUserId,now);if(issued.IsFailure)return Result.Failure<IssueCreditNoteResponse>(issued.Error);var inv=invoice.RecordCreditNoteIssued(note.TotalAmount,note.Currency,c.ActorUserId,now);if(inv.IsFailure)return Result.Failure<IssueCreditNoteResponse>(inv.Error);var acc=account.RecordCreditNoteIssued(note.TotalAmount,note.Currency,c.ActorUserId,now);if(acc.IsFailure)return Result.Failure<IssueCreditNoteResponse>(acc.Error);note.SetModifiedAudit(now,c.ActorUserId);invoice.SetModifiedAudit(now,c.ActorUserId);account.SetModifiedAudit(now,c.ActorUserId);await uow.CommitAsync(ct);return Result.Success(new IssueCreditNoteResponse(note.Id,note.CreditNoteNumber!,note.IssueDate!.Value,note.TotalAmount,note.Currency));}
}
