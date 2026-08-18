using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts.Events;

public sealed record BillingAccountSuspendedDomainEvent(
    BillingAccountId BillingAccountId,
    string Reason,
    UserId ActorUserId,
    DateTimeOffset SuspendedAtUtc) : DomainEvent;
