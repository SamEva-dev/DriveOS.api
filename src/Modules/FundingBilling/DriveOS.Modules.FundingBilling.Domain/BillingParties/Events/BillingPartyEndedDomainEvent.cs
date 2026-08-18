using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingParties.Events;

public sealed record BillingPartyEndedDomainEvent(BillingPartyId BillingPartyId, string Reason, UserId ActorUserId, DateTimeOffset EndedAtUtc) : DomainEvent;
