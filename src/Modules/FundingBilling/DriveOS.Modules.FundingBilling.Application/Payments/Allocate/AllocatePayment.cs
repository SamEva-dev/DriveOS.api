using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.Invoices;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Payments.Allocate;

public sealed record AllocatePaymentCommand(OrganizationId OrganizationId, PaymentId PaymentId,
    InvoiceId? InvoiceId, PaymentInstallmentId? InstallmentId, decimal Amount, UserId ActorUserId) : ICommand<PaymentAllocationId>;

internal sealed class AllocatePaymentCommandValidator : AbstractValidator<AllocatePaymentCommand>
{
    public AllocatePaymentCommandValidator()
    {
        RuleFor(x=>x.PaymentId.Value).NotEmpty(); RuleFor(x=>x.Amount).GreaterThan(0m); RuleFor(x=>x.ActorUserId.Value).NotEmpty();
        RuleFor(x=>x).Must(x=>x.InvoiceId.HasValue ^ x.InstallmentId.HasValue).WithMessage("Exactly one allocation target must be specified.");
    }
}

internal sealed class AllocatePaymentCommandHandler(IPaymentRepository payments, IInvoiceRepository invoices,
    IPaymentInstallmentRepository installments, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<AllocatePaymentCommand, PaymentAllocationId>
{
    public async Task<Result<PaymentAllocationId>> Handle(AllocatePaymentCommand c, CancellationToken ct)
    {
        Payment? payment=await payments.GetByIdAsync(c.PaymentId,ct);
        if(payment is null||payment.OrganizationId!=c.OrganizationId)return Result.Failure<PaymentAllocationId>(PaymentErrors.NotFound);
        DateTimeOffset now=clock.UtcNow;

        if(c.InvoiceId is { } invoiceId)
        {
            Invoice? invoice=await invoices.GetByIdAsync(invoiceId,ct);
            if(invoice is null||invoice.OrganizationId!=c.OrganizationId)return Result.Failure<PaymentAllocationId>(PaymentErrors.AllocationTargetNotFound);
            if(invoice.BillingAccountId!=payment.BillingAccountId)return Result.Failure<PaymentAllocationId>(PaymentErrors.AllocationBillingAccountMismatch);
            if(invoice.Currency!=payment.Currency)return Result.Failure<PaymentAllocationId>(PaymentErrors.CurrencyMismatch);
            Result target=invoice.RecordPaymentAllocation(c.Amount,payment.Currency,c.ActorUserId,now);
            if(target.IsFailure)return Result.Failure<PaymentAllocationId>(target.Error);
            Result<PaymentAllocationId> allocated=payment.Allocate(PaymentAllocationId.New(),invoiceId,null,c.Amount,c.ActorUserId,now);
            if(allocated.IsFailure)return allocated;
            invoice.SetModifiedAudit(now,c.ActorUserId); payment.SetModifiedAudit(now,c.ActorUserId); await uow.CommitAsync(ct); return allocated;
        }

        PaymentInstallment? installment=await installments.GetByIdAsync(c.InstallmentId!.Value,ct);
        if(installment is null||installment.OrganizationId!=c.OrganizationId)return Result.Failure<PaymentAllocationId>(PaymentErrors.AllocationTargetNotFound);
        if(installment.BillingAccountId!=payment.BillingAccountId)return Result.Failure<PaymentAllocationId>(PaymentErrors.AllocationBillingAccountMismatch);
        if(installment.Currency!=payment.Currency)return Result.Failure<PaymentAllocationId>(PaymentErrors.CurrencyMismatch);
        Result installmentResult=installment.RecordPaymentAllocation(c.Amount,payment.Currency,c.ActorUserId,now);
        if(installmentResult.IsFailure)return Result.Failure<PaymentAllocationId>(installmentResult.Error);
        Result<PaymentAllocationId> allocation=payment.Allocate(PaymentAllocationId.New(),null,c.InstallmentId,c.Amount,c.ActorUserId,now);
        if(allocation.IsFailure)return allocation;
        installment.SetModifiedAudit(now,c.ActorUserId); payment.SetModifiedAudit(now,c.ActorUserId); await uow.CommitAsync(ct); return allocation;
    }
}
