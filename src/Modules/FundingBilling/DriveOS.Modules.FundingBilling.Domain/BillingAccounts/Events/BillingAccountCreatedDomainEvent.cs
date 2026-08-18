using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts.Events;

public sealed record BillingAccountCreatedDomainEvent(
    BillingAccountId BillingAccountId,
    OrganizationId OrganizationId,
    PersonId StudentId,
    string Currency) : DomainEvent;
