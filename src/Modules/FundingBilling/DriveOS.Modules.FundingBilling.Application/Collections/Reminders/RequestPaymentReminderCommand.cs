using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.FundingBilling.Application.Collections.Reminders;
public sealed record RequestPaymentReminderCommand(OrganizationId OrganizationId, PaymentReminderTargetType TargetType, Guid TargetId, UserId ActorUserId) : ICommand<PaymentReminderId>;
