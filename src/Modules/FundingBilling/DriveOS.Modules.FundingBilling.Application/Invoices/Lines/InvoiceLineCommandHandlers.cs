using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Invoices.Lines;

internal sealed class AddInvoiceLineCommandHandler(
    IInvoiceRepository invoices,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AddInvoiceLineCommand, InvoiceLineId>
{
    public async Task<Result<InvoiceLineId>> Handle(AddInvoiceLineCommand command, CancellationToken cancellationToken)
    {
        Invoice? invoice = await invoices.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null || invoice.OrganizationId != command.OrganizationId)
            return Result.Failure<InvoiceLineId>(InvoiceErrors.NotFound);

        Result<InvoiceLineId> result = invoice.AddLine(
            InvoiceLineId.New(), command.Description, command.Quantity, command.Unit,
            command.UnitPrice, command.DiscountAmount, command.TaxRate);
        if (result.IsFailure) return result;

        invoice.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return result;
    }
}

internal sealed class RemoveInvoiceLineCommandHandler(
    IInvoiceRepository invoices,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RemoveInvoiceLineCommand>
{
    public async Task<Result> Handle(RemoveInvoiceLineCommand command, CancellationToken cancellationToken)
    {
        Invoice? invoice = await invoices.GetByIdAsync(command.InvoiceId, cancellationToken);
        if (invoice is null || invoice.OrganizationId != command.OrganizationId)
            return Result.Failure(InvoiceErrors.NotFound);

        Result result = invoice.RemoveLine(command.InvoiceLineId);
        if (result.IsFailure) return result;

        invoice.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
