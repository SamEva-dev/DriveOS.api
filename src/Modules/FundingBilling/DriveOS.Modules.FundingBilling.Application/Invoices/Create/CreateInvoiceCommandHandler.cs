using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Create;

internal sealed class CreateInvoiceCommandHandler(
    IStudentBillingAccountRepository accounts,
    IInvoiceRepository invoices,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateInvoiceCommand, InvoiceId>
{
    public async Task<Result<InvoiceId>> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        BillingAccount? account = await accounts.GetByIdAsync(command.BillingAccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<InvoiceId>(InvoiceErrors.BillingAccountNotFound);

        if (account.Status == BillingAccountStatus.Closed)
            return Result.Failure<InvoiceId>(InvoiceErrors.BillingAccountClosed);

        Result<Invoice> created = Invoice.CreateDraft(
            InvoiceId.New(),
            command.OrganizationId,
            account.Id,
            account.StudentId,
            account.Currency);

        if (created.IsFailure)
            return Result.Failure<InvoiceId>(created.Error);

        foreach (CreateInvoiceLineInput input in command.Lines)
        {
            Result<InvoiceLineId> added = created.Value.AddLine(
                InvoiceLineId.New(),
                input.Description,
                input.Quantity,
                input.Unit,
                input.UnitPrice,
                input.DiscountAmount,
                input.TaxRate);

            if (added.IsFailure)
                return Result.Failure<InvoiceId>(added.Error);
        }

        created.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        await invoices.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}
