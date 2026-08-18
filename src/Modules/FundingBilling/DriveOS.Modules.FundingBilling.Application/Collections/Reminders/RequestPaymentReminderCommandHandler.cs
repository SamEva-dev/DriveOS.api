using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.FundingBilling.Domain.Collections;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.Collections.Reminders;
internal sealed class RequestPaymentReminderCommandHandler(IInvoiceRepository invoices, IPaymentInstallmentRepository installments, IPaymentReminderRepository reminders, IFundingBillingUnitOfWork unitOfWork, IFinancialNotificationGateway notifications, IClock clock) : ICommandHandler<RequestPaymentReminderCommand, PaymentReminderId>
{
    public async Task<Result<PaymentReminderId>> Handle(RequestPaymentReminderCommand command, CancellationToken cancellationToken)
    {
        BillingAccountId accountId; DateOnly dueDate; decimal remaining; string currency;
        if (command.TargetType == PaymentReminderTargetType.Invoice)
        {
            Invoice? invoice = await invoices.GetByIdAsync(new InvoiceId(command.TargetId), cancellationToken);
            if (invoice is null || invoice.OrganizationId != command.OrganizationId) return Result.Failure<PaymentReminderId>(InvoiceErrors.NotFound);
            if (invoice.Status != InvoiceStatus.Overdue || invoice.DueDate is null || invoice.RemainingAmount <= 0m) return Result.Failure<PaymentReminderId>(PaymentReminderErrors.InvalidTarget);
            accountId = invoice.BillingAccountId; dueDate = invoice.DueDate.Value; remaining = invoice.RemainingAmount; currency = invoice.Currency;
        }
        else if (command.TargetType == PaymentReminderTargetType.Installment)
        {
            PaymentInstallment? installment = await installments.GetByIdAsync(new PaymentInstallmentId(command.TargetId), cancellationToken);
            if (installment is null || installment.OrganizationId != command.OrganizationId) return Result.Failure<PaymentReminderId>(PaymentInstallmentErrors.NotFound);
            if (installment.Status != PaymentInstallmentStatus.Overdue || installment.RemainingAmount <= 0m) return Result.Failure<PaymentReminderId>(PaymentReminderErrors.InvalidTarget);
            accountId = installment.BillingAccountId; dueDate = installment.DueDate; remaining = installment.RemainingAmount; currency = installment.Currency;
        }
        else return Result.Failure<PaymentReminderId>(PaymentReminderErrors.InvalidTarget);

        if (await reminders.HasPendingAsync(command.OrganizationId, command.TargetType, command.TargetId, cancellationToken)) return Result.Failure<PaymentReminderId>(PaymentReminderErrors.DuplicatePending);
        int sequence = await reminders.CountForTargetAsync(command.OrganizationId, command.TargetType, command.TargetId, cancellationToken) + 1;
        Result<PaymentReminder> result = PaymentReminder.Request(PaymentReminderId.New(), command.OrganizationId, accountId, command.TargetType, command.TargetId, dueDate, remaining, currency, sequence, clock.UtcNow);
        if (result.IsFailure) return Result.Failure<PaymentReminderId>(result.Error);
        result.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        await reminders.AddAsync(result.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        Guid? emailMessageId = await notifications.QueuePaymentReminderAsync(
            command.OrganizationId,
            accountId,
            command.TargetType.ToString(),
            remaining,
            currency,
            dueDate,
            sequence,
            cancellationToken);

        if (emailMessageId.HasValue)
        {
            Result marked = result.Value.MarkSent(emailMessageId.Value, clock.UtcNow);
            if (marked.IsSuccess)
            {
                result.Value.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
                await unitOfWork.CommitAsync(cancellationToken);
            }
        }

        return Result.Success(result.Value.Id);
    }
}
