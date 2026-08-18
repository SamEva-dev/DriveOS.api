using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.CreditNotes;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.CreditNotes.Read;
public sealed record CreditNoteLineResponse(Guid Id,Guid? InvoiceLineId,string Description,decimal Quantity,string Unit,decimal UnitPrice,decimal DiscountAmount,decimal TaxRate,decimal NetAmount,decimal TaxAmount,decimal TotalAmount);
public sealed record CreditNoteResponse(Guid Id,Guid InvoiceId,Guid BillingAccountId,string Currency,string Reason,string? CreditNoteNumber,DateOnly? IssueDate,string Status,decimal Subtotal,decimal TaxAmount,decimal TotalAmount,DateTimeOffset CreatedAtUtc,DateTimeOffset? IssuedAtUtc,IReadOnlyCollection<CreditNoteLineResponse> Lines);
public interface ICreditNoteReadService{Task<CreditNoteResponse?> GetAsync(OrganizationId organizationId,CreditNoteId id,CancellationToken cancellationToken=default);Task<IReadOnlyCollection<CreditNoteResponse>> ListByInvoiceAsync(OrganizationId organizationId,InvoiceId invoiceId,CancellationToken cancellationToken=default);}
public sealed record GetCreditNoteQuery(OrganizationId OrganizationId,CreditNoteId CreditNoteId):IQuery<CreditNoteResponse>;
public sealed record GetInvoiceCreditNotesQuery(OrganizationId OrganizationId,InvoiceId InvoiceId):IQuery<IReadOnlyCollection<CreditNoteResponse>>;
internal sealed class GetCreditNoteQueryHandler(ICreditNoteReadService read):IQueryHandler<GetCreditNoteQuery,CreditNoteResponse>{public async Task<Result<CreditNoteResponse>> Handle(GetCreditNoteQuery q,CancellationToken ct){var x=await read.GetAsync(q.OrganizationId,q.CreditNoteId,ct);return x is null?Result.Failure<CreditNoteResponse>(CreditNoteErrors.NotFound):Result.Success(x);}}
internal sealed class GetInvoiceCreditNotesQueryHandler(ICreditNoteReadService read):IQueryHandler<GetInvoiceCreditNotesQuery,IReadOnlyCollection<CreditNoteResponse>>{public async Task<Result<IReadOnlyCollection<CreditNoteResponse>>> Handle(GetInvoiceCreditNotesQuery q,CancellationToken ct)=>Result.Success(await read.ListByInvoiceAsync(q.OrganizationId,q.InvoiceId,ct));}
