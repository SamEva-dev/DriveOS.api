using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts.Events;

public sealed record BillingAccountReactivatedDomainEvent(
    BillingAccountId BillingAccountId,
    UserId ActorUserId,
    DateTimeOffset ReactivatedAtUtc) : DomainEvent;
