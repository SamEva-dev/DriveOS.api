using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Application.Notifications;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Issue;

internal sealed class IssueInvoiceCommandHandler(
    IInvoiceRepository invoices,
    IStudentBillingAccountRepository accounts,
    IInvoiceNumberGenerator numberGenerator,
    IFundingBillingUnitOfWork unitOfWork,
    IFinancialNotificationGateway notifications,
    IClock clock) : ICommandHandler<IssueInvoiceCommand, IssueInvoiceResponse>
{
    public async Task<Result<IssueInvoiceResponse>> Handle(IssueInvoiceCommand command, CancellationToken cancellationToken)
    {
        Invoice? invoice = await invoices.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null || invoice.OrganizationId != command.OrganizationId)
            return Result.Failure<IssueInvoiceResponse>(InvoiceErrors.NotFound);

        BillingAccount? account = await accounts.GetByIdAsync(invoice.BillingAccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<IssueInvoiceResponse>(InvoiceErrors.BillingAccountNotFound);

        if (account.Status == BillingAccountStatus.Closed)
            return Result.Failure<IssueInvoiceResponse>(InvoiceErrors.BillingAccountClosed);

        Result<string> number = await numberGenerator.ReserveNextAsync(command.OrganizationId, cancellationToken);
        if (number.IsFailure)
            return Result.Failure<IssueInvoiceResponse>(number.Error);

        DateTimeOffset now = clock.UtcNow;
        Result issued = invoice.Issue(number.Value, command.IssueDate, command.DueDate, command.ActorUserId, now);
        if (issued.IsFailure)
            return Result.Failure<IssueInvoiceResponse>(issued.Error);

        Result accountUpdated = account.RecordInvoiceIssued(invoice.TotalAmount, invoice.Currency, command.ActorUserId, now);
        if (accountUpdated.IsFailure)
            return Result.Failure<IssueInvoiceResponse>(accountUpdated.Error);

        invoice.SetModifiedAudit(now, command.ActorUserId);
        account.SetModifiedAudit(now, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        await notifications.QueueInvoiceIssuedAsync(command.OrganizationId, account.Id, invoice.InvoiceNumber!, invoice.TotalAmount, invoice.Currency, invoice.DueDate!.Value, cancellationToken);

        return Result.Success(new IssueInvoiceResponse(
            invoice.Id,
            invoice.InvoiceNumber!,
            invoice.IssueDate!.Value,
            invoice.DueDate!.Value,
            invoice.TotalAmount,
            invoice.Currency));
    }
}
