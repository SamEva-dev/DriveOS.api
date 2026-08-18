using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.CreditNotes.Events;

public sealed record CreditNoteCreatedDomainEvent(CreditNoteId CreditNoteId, InvoiceId InvoiceId, BillingAccountId BillingAccountId, string Reason) : DomainEvent;
public sealed record CreditNoteIssuedDomainEvent(CreditNoteId CreditNoteId, InvoiceId InvoiceId, BillingAccountId BillingAccountId, string Number, decimal Amount, string Currency, UserId ActorUserId, DateTimeOffset CreditNoteIssuedAtUtc) : DomainEvent;
