using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.FundingBilling.Domain.Collections.Events;

public sealed record PaymentReminderRequestedDomainEvent(
    PaymentReminderId ReminderId,
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId,
    PaymentReminderTargetType TargetType,
    Guid TargetId,
    decimal OutstandingAmount,
    string Currency,
    DateOnly DueDate,
    DateTimeOffset RequestedAtUtc) : DomainEvent;
