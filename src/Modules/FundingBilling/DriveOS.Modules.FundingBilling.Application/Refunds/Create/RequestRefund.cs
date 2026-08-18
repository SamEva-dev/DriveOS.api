using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.Refunds;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Refunds.Create;

public sealed record RequestRefundCommand(OrganizationId OrganizationId, PaymentId PaymentId, decimal Amount, string Reason, UserId ActorUserId) : ICommand<RefundId>;
internal sealed class RequestRefundCommandValidator : AbstractValidator<RequestRefundCommand>
{
    public RequestRefundCommandValidator() { RuleFor(x=>x.PaymentId.Value).NotEmpty(); RuleFor(x=>x.Amount).GreaterThan(0m); RuleFor(x=>x.Reason).NotEmpty().MaximumLength(1000); RuleFor(x=>x.ActorUserId.Value).NotEmpty(); }
}
internal sealed class RequestRefundCommandHandler(IPaymentRepository payments, IRefundRepository refunds, IFundingBillingUnitOfWork uow, IClock clock) : ICommandHandler<RequestRefundCommand, RefundId>
{
    public async Task<Result<RefundId>> Handle(RequestRefundCommand c, CancellationToken ct)
    {
        Payment? payment = await payments.GetByIdAsync(c.PaymentId, ct);
        if (payment is null || payment.OrganizationId != c.OrganizationId) return Result.Failure<RefundId>(RefundErrors.PaymentNotFound);
        if (payment.Status is not (PaymentStatus.Paid or PaymentStatus.PartiallyRefunded)) return Result.Failure<RefundId>(RefundErrors.PaymentNotRefundable);
        decimal reserved = await refunds.GetReservedAmountForPaymentAsync(payment.Id, null, ct);
        if (c.Amount > payment.Amount - reserved) return Result.Failure<RefundId>(RefundErrors.AmountExceeded);
        DateTimeOffset now = clock.UtcNow; var created = Refund.Request(RefundId.New(), c.OrganizationId, payment.BillingAccountId, payment.Id, c.Amount, payment.Currency, c.Reason, c.ActorUserId, now);
        if (created.IsFailure) return Result.Failure<RefundId>(created.Error);
        created.Value.SetCreatedAudit(now, c.ActorUserId); await refunds.AddAsync(created.Value, ct); await uow.CommitAsync(ct); return Result.Success(created.Value.Id);
    }
}
