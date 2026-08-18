using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Invoices.Events;

public sealed record InvoiceCreatedDomainEvent(
    InvoiceId InvoiceId,
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId,
    PersonId CustomerPersonId,
    string Currency) : DomainEvent;
