using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.BillingAccounts.Events;

public sealed record BillingAccountClosedDomainEvent(
    BillingAccountId BillingAccountId,
    string Reason,
    UserId ActorUserId,
    DateTimeOffset ClosedAtUtc) : DomainEvent;
