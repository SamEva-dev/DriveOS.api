using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingParties.Events;

public sealed record BillingPartyAddedDomainEvent(BillingPartyId BillingPartyId, BillingAccountId BillingAccountId, BillingPartyRole Role) : DomainEvent;
