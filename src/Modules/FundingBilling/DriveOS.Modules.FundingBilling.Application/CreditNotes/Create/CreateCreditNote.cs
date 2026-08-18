using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.CreditNotes.Create;

public sealed record CreateCreditNoteLineRequest(InvoiceLineId? InvoiceLineId,string Description,decimal Quantity,string Unit,decimal UnitPrice,decimal DiscountAmount,decimal TaxRate);
public sealed record CreateCreditNoteCommand(OrganizationId OrganizationId,InvoiceId InvoiceId,string Reason,IReadOnlyCollection<CreateCreditNoteLineRequest> Lines,UserId ActorUserId):ICommand<CreditNoteId>;
internal sealed class CreateCreditNoteCommandValidator:AbstractValidator<CreateCreditNoteCommand>{public CreateCreditNoteCommandValidator(){RuleFor(x=>x.InvoiceId.Value).NotEmpty();RuleFor(x=>x.Reason).NotEmpty().MaximumLength(1000);RuleFor(x=>x.Lines).NotEmpty();RuleFor(x=>x.ActorUserId.Value).NotEmpty();}}
internal sealed class CreateCreditNoteCommandHandler(IInvoiceRepository invoices,ICreditNoteRepository notes,IFundingBillingUnitOfWork uow,IClock clock):ICommandHandler<CreateCreditNoteCommand,CreditNoteId>
{
 public async Task<Result<CreditNoteId>> Handle(CreateCreditNoteCommand c,CancellationToken ct){var invoice=await invoices.GetByIdAsync(c.InvoiceId,ct);if(invoice is null||invoice.OrganizationId!=c.OrganizationId)return Result.Failure<CreditNoteId>(CreditNoteErrors.InvoiceNotFound);if(invoice.Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled||invoice.CreditableAmount<=0)return Result.Failure<CreditNoteId>(CreditNoteErrors.InvoiceNotCreditable);var created=CreditNote.CreateDraft(CreditNoteId.New(),c.OrganizationId,invoice.BillingAccountId,invoice.Id,invoice.Currency,c.Reason);if(created.IsFailure)return Result.Failure<CreditNoteId>(created.Error);foreach(var l in c.Lines){var r=created.Value.AddLine(CreditNoteLineId.New(),l.InvoiceLineId,l.Description,l.Quantity,l.Unit,l.UnitPrice,l.DiscountAmount,l.TaxRate);if(r.IsFailure)return Result.Failure<CreditNoteId>(r.Error);}if(created.Value.TotalAmount>invoice.CreditableAmount)return Result.Failure<CreditNoteId>(CreditNoteErrors.AmountExceeded);var now=clock.UtcNow;created.Value.SetCreatedAudit(now,c.ActorUserId);await notes.AddAsync(created.Value,ct);await uow.CommitAsync(ct);return Result.Success(created.Value.Id);}
}
