using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.SupplierInvoices;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.SupplierInvoices;

public sealed class ReceiveSupplierInvoiceCommandHandler(
    ISupplierInvoiceRepository invoices,
    IFundingBillingUnitOfWork uow,
    IClock clock) : ICommandHandler<ReceiveSupplierInvoiceCommand, SupplierInvoiceId>
{
    public async Task<Result<SupplierInvoiceId>> Handle(ReceiveSupplierInvoiceCommand c, CancellationToken ct)
    {
        SupplierInvoice? existing = await invoices.GetByExternalSourceAsync(c.SourceType, c.ExternalSourceId, false, ct);
        if(existing is not null)
            return Result.Success(existing.Id);

        Result<SupplierInvoice> received = SupplierInvoice.Receive(
            c.Id,c.ClientOrganizationId,c.SupplierOrganizationId,c.SourceType,c.ExternalSourceId,c.ServiceStatementId,
            c.SupplierReference,c.IssueDate,c.DueDate,c.Currency,c.Subtotal,c.TaxAmount,c.InvoiceMode,clock.UtcNow,c.ActorUserId);

        if(received.IsFailure)return Result.Failure<SupplierInvoiceId>(received.Error);

        invoices.Add(received.Value);
        await uow.CommitAsync(ct);
        return Result.Success(received.Value.Id);
    }
}

public abstract class SupplierInvoiceMutation
{
    protected static async Task<Result> Run(
        SupplierInvoiceId id,
        OrganizationId organizationId,
        Func<SupplierInvoice,Result> action,
        ISupplierInvoiceRepository repo,
        IFundingBillingUnitOfWork uow,
        CancellationToken ct)
    {
        SupplierInvoice? invoice=await repo.GetAsync(id,true,ct);
        if(invoice is null||invoice.ClientOrganizationId!=organizationId)
            return Result.Failure(SupplierInvoiceErrors.NotFound);

        Result result=action(invoice);
        if(result.IsFailure)return result;

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class MatchSupplierInvoiceCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<MatchSupplierInvoiceCommand>
{
    public Task<Result> Handle(MatchSupplierInvoiceCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.MarkMatched(c.ActorUserId,clock.UtcNow),repo,uow,ct);
}

public sealed class ApproveSupplierInvoiceOperationalCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<ApproveSupplierInvoiceOperationalCommand>
{
    public Task<Result> Handle(ApproveSupplierInvoiceOperationalCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.ApproveOperational(c.ActorUserId,clock.UtcNow),repo,uow,ct);
}

public sealed class ApproveSupplierInvoiceFinancialCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<ApproveSupplierInvoiceFinancialCommand>
{
    public Task<Result> Handle(ApproveSupplierInvoiceFinancialCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.ApproveFinancial(c.ActorUserId,clock.UtcNow),repo,uow,ct);
}

public sealed class ScheduleSupplierInvoicePaymentCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<ScheduleSupplierInvoicePaymentCommand>
{
    public Task<Result> Handle(ScheduleSupplierInvoicePaymentCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.SchedulePayment(x.RemainingAmount,c.ActorUserId,clock.UtcNow),repo,uow,ct);
}

public sealed class MarkSupplierInvoicePaidCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<MarkSupplierInvoicePaidCommand>
{
    public Task<Result> Handle(MarkSupplierInvoicePaidCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.ApplySettledPayment(x.RemainingAmount,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class RejectSupplierInvoiceCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<RejectSupplierInvoiceCommand>
{
    public Task<Result> Handle(RejectSupplierInvoiceCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.Reject(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}

public sealed class DisputeSupplierInvoiceCommandHandler(
    ISupplierInvoiceRepository repo,IFundingBillingUnitOfWork uow,IClock clock)
    :SupplierInvoiceMutation,ICommandHandler<DisputeSupplierInvoiceCommand>
{
    public Task<Result> Handle(DisputeSupplierInvoiceCommand c,CancellationToken ct)=>
        Run(c.Id,c.ClientOrganizationId,x=>x.Dispute(c.Reason,clock.UtcNow,c.ActorUserId),repo,uow,ct);
}
