using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.Collections.DetectOverdue;
internal sealed class DetectOverdueCommandHandler(IInvoiceRepository invoices, IPaymentInstallmentRepository installments, IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<DetectOverdueCommand, DetectOverdueResponse>
{
    public async Task<Result<DetectOverdueResponse>> Handle(DetectOverdueCommand command, CancellationToken cancellationToken)
    {
        var dueInvoices = await invoices.ListDueAsync(command.OrganizationId, command.BusinessDate, cancellationToken);
        var dueInstallments = await installments.ListDueAsync(command.OrganizationId, command.BusinessDate, cancellationToken);
        int invoiceCount = 0, installmentCount = 0;
        DateTimeOffset now = clock.UtcNow;
        foreach (Invoice invoice in dueInvoices)
        {
            Result result = invoice.MarkOverdue(command.BusinessDate, now);
            if (result.IsSuccess) { invoice.SetModifiedAudit(now, command.ActorUserId); invoiceCount++; }
        }
        foreach (PaymentInstallment installment in dueInstallments)
        {
            Result result = installment.MarkOverdue(command.BusinessDate, now);
            if (result.IsSuccess) { installment.SetModifiedAudit(now, command.ActorUserId); installmentCount++; }
        }
        if (invoiceCount + installmentCount > 0) await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(new DetectOverdueResponse(invoiceCount, installmentCount));
    }
}
