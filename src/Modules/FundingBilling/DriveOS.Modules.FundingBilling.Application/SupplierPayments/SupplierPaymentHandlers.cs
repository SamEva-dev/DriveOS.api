using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.Modules.FundingBilling.Domain.SupplierPayments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.SupplierPayments;

public sealed class ScheduleSupplierPaymentCommandHandler(
    ISupplierInvoiceRepository invoices,
    ISupplierPaymentAttemptRepository attempts,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<ScheduleSupplierPaymentCommand,SupplierPaymentAttemptId>
{
    public async Task<Result<SupplierPaymentAttemptId>> Handle(ScheduleSupplierPaymentCommand c,CancellationToken ct)
    {
        SupplierInvoice? invoice=await invoices.GetAsync(c.SupplierInvoiceId,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure<SupplierPaymentAttemptId>(SupplierInvoiceErrors.NotFound);

        if(invoice.Status!=SupplierInvoiceStatus.Approved)
            return Result.Failure<SupplierPaymentAttemptId>(SupplierPaymentAttemptErrors.InvoiceNotApproved);

        if(await attempts.HasActiveAttemptAsync(invoice.Id,ct))
            return Result.Failure<SupplierPaymentAttemptId>(SupplierPaymentAttemptErrors.ActiveAttemptExists);

        decimal amount=decimal.Round(c.Amount??invoice.RemainingAmount,2,MidpointRounding.AwayFromZero);
        if(amount<=0||amount>invoice.RemainingAmount)
            return Result.Failure<SupplierPaymentAttemptId>(SupplierInvoiceErrors.InvalidSettlementAmount);

        var scheduled=SupplierPaymentAttempt.Schedule(
            c.Id,invoice.Id,invoice.ClientOrganizationId,invoice.SupplierOrganizationId,
            amount,invoice.Currency,c.PaymentMethod,c.ScheduledDate,c.BankReference,
            clock.UtcNow,c.ActorUserId,c.BatchId,false);

        if(scheduled.IsFailure)return Result.Failure<SupplierPaymentAttemptId>(scheduled.Error);

        Result marked=invoice.SchedulePayment(amount,c.ActorUserId,clock.UtcNow);
        if(marked.IsFailure)return Result.Failure<SupplierPaymentAttemptId>(marked.Error);

        attempts.Add(scheduled.Value);
        await uow.CommitAsync(ct);
        return Result.Success(scheduled.Value.Id);
    }
}

public sealed class MarkSupplierPaymentProcessingCommandHandler(
    ISupplierPaymentAttemptRepository attempts,
    ISupplierInvoiceRepository invoices,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<MarkSupplierPaymentProcessingCommand>
{
    public async Task<Result> Handle(MarkSupplierPaymentProcessingCommand c,CancellationToken ct)
    {
        SupplierPaymentAttempt? attempt=await attempts.GetAsync(c.Id,true,ct);
        if(attempt is null||attempt.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure(SupplierPaymentAttemptErrors.NotFound);

        SupplierInvoice? invoice=await invoices.GetAsync(attempt.SupplierInvoiceId,true,ct);
        if(invoice is null)return Result.Failure(SupplierInvoiceErrors.NotFound);

        Result processing=attempt.MarkProcessing(clock.UtcNow,c.ActorUserId);
        if(processing.IsFailure)return processing;

        Result invoiceProcessing=invoice.MarkPaymentProcessing(clock.UtcNow,c.ActorUserId);
        if(invoiceProcessing.IsFailure)return invoiceProcessing;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class MarkSupplierPaymentFailedCommandHandler(
    ISupplierPaymentAttemptRepository attempts,
    ISupplierInvoiceRepository invoices,
    ISupplierFinanceNotificationGateway notifications,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<MarkSupplierPaymentFailedCommand>
{
    public async Task<Result> Handle(MarkSupplierPaymentFailedCommand c,CancellationToken ct)
    {
        SupplierPaymentAttempt? attempt=await attempts.GetAsync(c.Id,true,ct);
        if(attempt is null||attempt.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure(SupplierPaymentAttemptErrors.NotFound);

        SupplierInvoice? invoice=await invoices.GetAsync(attempt.SupplierInvoiceId,true,ct);
        if(invoice is null)return Result.Failure(SupplierInvoiceErrors.NotFound);

        Result failed=attempt.MarkFailed(c.Reason,clock.UtcNow,c.ActorUserId);
        if(failed.IsFailure)return failed;

        Result invoiceFailed=invoice.MarkPaymentFailed(c.Reason,clock.UtcNow,c.ActorUserId);
        if(invoiceFailed.IsFailure)return invoiceFailed;

        await uow.CommitAsync(ct);

        await notifications.TryEnqueueAsync(new(
            invoice.SupplierOrganizationId,
            invoice.ClientOrganizationId,
            "professionalMarketplace.notifications.paymentFailed",
            $"supplier-payment-failed:{attempt.Id.Value}",
            new Dictionary<string,string?>
            {
                ["supplierInvoiceId"]=invoice.Id.Value.ToString(),
                ["amount"]=attempt.Amount.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),
                ["currency"]=attempt.Currency,
                ["reason"]=attempt.FailureReason
            },
            invoice.Id.Value,
            c.ActorUserId),ct);

        return Result.Success();
    }
}

public sealed class MarkSupplierPaymentPaidCommandHandler(
    ISupplierPaymentAttemptRepository attempts,
    ISupplierInvoiceRepository invoices,
    ISupplierFinanceNotificationGateway notifications,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<MarkSupplierPaymentPaidCommand>
{
    public async Task<Result> Handle(MarkSupplierPaymentPaidCommand c,CancellationToken ct)
    {
        SupplierPaymentAttempt? attempt=await attempts.GetAsync(c.Id,true,ct);
        if(attempt is null||attempt.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure(SupplierPaymentAttemptErrors.NotFound);

        SupplierInvoice? invoice=await invoices.GetAsync(attempt.SupplierInvoiceId,true,ct);
        if(invoice is null)return Result.Failure(SupplierInvoiceErrors.NotFound);

        decimal settledAmount=decimal.Round(c.SettledAmount??attempt.Amount,2,MidpointRounding.AwayFromZero);
        DateOnly settledOn=c.SettledOn??DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);

        Result paid=attempt.MarkPaid(settledAmount,settledOn,c.ProviderReference,clock.UtcNow,c.ActorUserId);
        if(paid.IsFailure)return paid;

        Result invoicePaid=invoice.ApplySettledPayment(settledAmount,clock.UtcNow,c.ActorUserId);
        if(invoicePaid.IsFailure)return invoicePaid;

        await uow.CommitAsync(ct);

        await notifications.TryEnqueueAsync(new(
            invoice.SupplierOrganizationId,
            invoice.ClientOrganizationId,
            invoice.SettlementStatus==SupplierInvoiceSettlementStatus.Paid
                ?"professionalMarketplace.notifications.paymentReceived"
                :"professionalMarketplace.notifications.paymentPartiallyReceived",
            $"supplier-payment-paid:{attempt.Id.Value}",
            new Dictionary<string,string?>
            {
                ["supplierInvoiceId"]=invoice.Id.Value.ToString(),
                ["amount"]=settledAmount.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),
                ["remainingAmount"]=invoice.RemainingAmount.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),
                ["currency"]=attempt.Currency,
                ["providerReference"]=attempt.ProviderReference,
                ["reconciliationStatus"]=attempt.ReconciliationStatus.ToString(),
                ["difference"]=attempt.ReconciliationDifference?.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture)
            },
            invoice.Id.Value,
            c.ActorUserId),ct);

        return Result.Success();
    }
}

public sealed class CancelSupplierPaymentAttemptCommandHandler(
    ISupplierPaymentAttemptRepository attempts,
    ISupplierInvoiceRepository invoices,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<CancelSupplierPaymentAttemptCommand>
{
    public async Task<Result> Handle(CancelSupplierPaymentAttemptCommand c,CancellationToken ct)
    {
        SupplierPaymentAttempt? attempt=await attempts.GetAsync(c.Id,true,ct);
        if(attempt is null||attempt.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure(SupplierPaymentAttemptErrors.NotFound);

        SupplierInvoice? invoice=await invoices.GetAsync(attempt.SupplierInvoiceId,true,ct);
        if(invoice is null)return Result.Failure(SupplierInvoiceErrors.NotFound);

        Result cancelled=attempt.Cancel(clock.UtcNow,c.ActorUserId);
        if(cancelled.IsFailure)return cancelled;

        Result invoiceCancelled=invoice.MarkPaymentCancelled("payment-attempt-cancelled",clock.UtcNow,c.ActorUserId);
        if(invoiceCancelled.IsFailure)return invoiceCancelled;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class RecordManualSupplierPaymentCommandHandler(
    ISupplierInvoiceRepository invoices,
    ISupplierPaymentAttemptRepository attempts,
    ISupplierFinanceNotificationGateway notifications,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<RecordManualSupplierPaymentCommand,SupplierPaymentAttemptId>
{
    public async Task<Result<SupplierPaymentAttemptId>> Handle(
        RecordManualSupplierPaymentCommand c,CancellationToken ct)
    {
        SupplierInvoice? invoice=await invoices.GetAsync(c.SupplierInvoiceId,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure<SupplierPaymentAttemptId>(SupplierInvoiceErrors.NotFound);

        if(invoice.Status!=SupplierInvoiceStatus.Approved)
            return Result.Failure<SupplierPaymentAttemptId>(SupplierPaymentAttemptErrors.InvoiceNotApproved);

        if(await attempts.HasActiveAttemptAsync(invoice.Id,ct))
            return Result.Failure<SupplierPaymentAttemptId>(SupplierPaymentAttemptErrors.ActiveAttemptExists);

        decimal amount=decimal.Round(c.Amount,2,MidpointRounding.AwayFromZero);
        if(amount<=0)
            return Result.Failure<SupplierPaymentAttemptId>(SupplierInvoiceErrors.InvalidSettlementAmount);

        var attemptResult=SupplierPaymentAttempt.Schedule(
            c.Id,invoice.Id,invoice.ClientOrganizationId,invoice.SupplierOrganizationId,
            amount,invoice.Currency,c.PaymentMethod,c.PaidOn,c.BankReference,
            clock.UtcNow,c.ActorUserId,null,true);

        if(attemptResult.IsFailure)
            return Result.Failure<SupplierPaymentAttemptId>(attemptResult.Error);

        Result scheduled=invoice.SchedulePayment(Math.Min(amount,invoice.RemainingAmount),c.ActorUserId,clock.UtcNow);
        if(scheduled.IsFailure)return Result.Failure<SupplierPaymentAttemptId>(scheduled.Error);

        Result paid=attemptResult.Value.MarkPaid(amount,c.PaidOn,c.ProviderReference,clock.UtcNow,c.ActorUserId);
        if(paid.IsFailure)return Result.Failure<SupplierPaymentAttemptId>(paid.Error);

        Result applied=invoice.ApplySettledPayment(amount,clock.UtcNow,c.ActorUserId);
        if(applied.IsFailure)return Result.Failure<SupplierPaymentAttemptId>(applied.Error);

        attempts.Add(attemptResult.Value);
        await uow.CommitAsync(ct);

        await notifications.TryEnqueueAsync(new(
            invoice.SupplierOrganizationId,invoice.ClientOrganizationId,
            invoice.SettlementStatus==SupplierInvoiceSettlementStatus.Paid
                ?"professionalMarketplace.notifications.paymentReceived"
                :"professionalMarketplace.notifications.paymentPartiallyReceived",
            $"supplier-manual-payment:{attemptResult.Value.Id.Value}",
            new Dictionary<string,string?>
            {
                ["supplierInvoiceId"]=invoice.Id.Value.ToString(),
                ["amount"]=amount.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),
                ["remainingAmount"]=invoice.RemainingAmount.ToString("0.00",System.Globalization.CultureInfo.InvariantCulture),
                ["currency"]=invoice.Currency,
                ["manual"]="true"
            },
            invoice.Id.Value,c.ActorUserId),ct);

        return Result.Success(attemptResult.Value.Id);
    }
}

public sealed class RecordSupplierPaymentRefundCommandHandler(
    ISupplierInvoiceRepository invoices,
    ISupplierPaymentRefundRepository refunds,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<RecordSupplierPaymentRefundCommand,SupplierPaymentRefundId>
{
    public async Task<Result<SupplierPaymentRefundId>> Handle(
        RecordSupplierPaymentRefundCommand c,CancellationToken ct)
    {
        SupplierInvoice? invoice=await invoices.GetAsync(c.SupplierInvoiceId,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=c.ClientOrganizationId)
            return Result.Failure<SupplierPaymentRefundId>(SupplierInvoiceErrors.NotFound);

        Result refundApplied=invoice.RecordRefund(c.Amount,c.Reason,clock.UtcNow,c.ActorUserId);
        if(refundApplied.IsFailure)
            return Result.Failure<SupplierPaymentRefundId>(refundApplied.Error);

        var recorded=SupplierPaymentRefund.Record(
            c.Id,invoice.Id,invoice.ClientOrganizationId,invoice.SupplierOrganizationId,
            c.Amount,invoice.Currency,c.Reason,c.Method,c.ProviderReference,clock.UtcNow,c.ActorUserId);

        if(recorded.IsFailure)
            return Result.Failure<SupplierPaymentRefundId>(recorded.Error);

        refunds.Add(recorded.Value);
        await uow.CommitAsync(ct);
        return Result.Success(recorded.Value.Id);
    }
}

public sealed class ScheduleSupplierPaymentBatchCommandHandler(
    ISupplierInvoiceRepository invoices,
    ISupplierPaymentAttemptRepository attempts,
    ISupplierPaymentBatchRepository batches,
    IFundingBillingUnitOfWork uow,
    IClock clock):ICommandHandler<ScheduleSupplierPaymentBatchCommand,SupplierPaymentBatchId>
{
    public async Task<Result<SupplierPaymentBatchId>> Handle(
        ScheduleSupplierPaymentBatchCommand c,CancellationToken ct)
    {
        if(c.Items is null||c.Items.Length==0||
           c.Items.Select(x=>x.SupplierInvoiceId).Distinct().Count()!=c.Items.Length)
            return Result.Failure<SupplierPaymentBatchId>(SupplierPaymentAttemptErrors.InvalidAmount);

        var resolved=new List<(SupplierInvoice Invoice,decimal Amount)>();
        string? currency=null;

        foreach(var item in c.Items)
        {
            SupplierInvoice? invoice=await invoices.GetAsync(item.SupplierInvoiceId,true,ct);
            if(invoice is null||invoice.ClientOrganizationId!=c.ClientOrganizationId)
                return Result.Failure<SupplierPaymentBatchId>(SupplierInvoiceErrors.NotFound);

            if(invoice.Status!=SupplierInvoiceStatus.Approved)
                return Result.Failure<SupplierPaymentBatchId>(SupplierPaymentAttemptErrors.InvoiceNotApproved);

            if(await attempts.HasActiveAttemptAsync(invoice.Id,ct))
                return Result.Failure<SupplierPaymentBatchId>(SupplierPaymentAttemptErrors.ActiveAttemptExists);

            currency??=invoice.Currency;
            if(!string.Equals(currency,invoice.Currency,StringComparison.Ordinal))
                return Result.Failure<SupplierPaymentBatchId>(SupplierPaymentAttemptErrors.MixedCurrenciesNotAllowed);

            decimal amount=decimal.Round(item.Amount??invoice.RemainingAmount,2,MidpointRounding.AwayFromZero);
            if(amount<=0||amount>invoice.RemainingAmount)
                return Result.Failure<SupplierPaymentBatchId>(SupplierInvoiceErrors.InvalidSettlementAmount);

            resolved.Add((invoice,amount));
        }

        var batchResult=SupplierPaymentBatch.Create(
            c.Id,c.ClientOrganizationId,c.PaymentMethod,currency!,c.ScheduledDate,c.BankReference,
            clock.UtcNow,c.ActorUserId);

        if(batchResult.IsFailure)
            return Result.Failure<SupplierPaymentBatchId>(batchResult.Error);

        decimal total=0m;
        foreach(var item in resolved)
        {
            var attempt=SupplierPaymentAttempt.Schedule(
                new SupplierPaymentAttemptId(Guid.NewGuid()),
                item.Invoice.Id,item.Invoice.ClientOrganizationId,item.Invoice.SupplierOrganizationId,
                item.Amount,item.Invoice.Currency,c.PaymentMethod,c.ScheduledDate,c.BankReference,
                clock.UtcNow,c.ActorUserId,c.Id,false);

            if(attempt.IsFailure)
                return Result.Failure<SupplierPaymentBatchId>(attempt.Error);

            Result scheduled=item.Invoice.SchedulePayment(item.Amount,c.ActorUserId,clock.UtcNow);
            if(scheduled.IsFailure)
                return Result.Failure<SupplierPaymentBatchId>(scheduled.Error);

            attempts.Add(attempt.Value);
            total+=item.Amount;
        }

        Result totals=batchResult.Value.SetTotals(resolved.Count,total,clock.UtcNow,c.ActorUserId);
        if(totals.IsFailure)
            return Result.Failure<SupplierPaymentBatchId>(totals.Error);

        batches.Add(batchResult.Value);
        await uow.CommitAsync(ct);
        return Result.Success(batchResult.Value.Id);
    }
}

public sealed class SupplierSettlementOverdueAutomation(
    ISupplierInvoiceRepository invoices,
    IFundingBillingUnitOfWork uow,
    IClock clock):ISupplierSettlementOverdueAutomation
{
    public async Task<int> RunAsync(CancellationToken cancellationToken=default)
    {
        DateOnly today=DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        IReadOnlyList<SupplierInvoice> items=await invoices.ListOverdueCandidatesAsync(today,true,cancellationToken);
        int updated=0;

        foreach(SupplierInvoice invoice in items)
        {
            if(invoice.MarkOverdue(today,clock.UtcNow).IsSuccess)
                updated++;
        }

        if(updated>0)
            await uow.CommitAsync(cancellationToken);

        return updated;
    }
}
