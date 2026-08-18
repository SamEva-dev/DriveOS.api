using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Collections.Reminders;

public sealed record DispatchPendingPaymentRemindersCommand(
    OrganizationId OrganizationId,
    int Take,
    UserId ActorUserId) : ICommand<DispatchPendingPaymentRemindersResponse>;

public sealed record DispatchPendingPaymentRemindersResponse(int Processed, int Queued, int Skipped);

internal sealed class DispatchPendingPaymentRemindersCommandHandler(
    IPaymentReminderRepository reminders,
    IFinancialNotificationGateway notifications,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<DispatchPendingPaymentRemindersCommand, DispatchPendingPaymentRemindersResponse>
{
    public async Task<Result<DispatchPendingPaymentRemindersResponse>> Handle(
        DispatchPendingPaymentRemindersCommand command,
        CancellationToken cancellationToken)
    {
        int take = Math.Clamp(command.Take, 1, 200);
        IReadOnlyCollection<PaymentReminder> pending = await reminders.ListPendingAsync(command.OrganizationId, take, cancellationToken);
        int queued = 0;
        int skipped = 0;

        foreach (PaymentReminder reminder in pending)
        {
            Guid? emailMessageId = await notifications.QueuePaymentReminderAsync(
                reminder.OrganizationId,
                reminder.BillingAccountId,
                reminder.TargetType.ToString(),
                reminder.OutstandingAmount,
                reminder.Currency,
                reminder.DueDate,
                reminder.SequenceNumber,
                cancellationToken);

            if (!emailMessageId.HasValue)
            {
                skipped++;
                continue;
            }

            DateTimeOffset now = clock.UtcNow;
            Result marked = reminder.MarkSent(emailMessageId.Value, now);
            if (marked.IsFailure)
            {
                skipped++;
                continue;
            }

            reminder.SetModifiedAudit(now, command.ActorUserId);
            await unitOfWork.CommitAsync(cancellationToken);
            queued++;
        }

        return Result.Success(new DispatchPendingPaymentRemindersResponse(pending.Count, queued, skipped));
    }
}
