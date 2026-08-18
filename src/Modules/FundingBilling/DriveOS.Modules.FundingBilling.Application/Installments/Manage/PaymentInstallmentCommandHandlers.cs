using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Manage;

internal sealed class ReschedulePaymentInstallmentCommandHandler(
    IPaymentInstallmentRepository repository,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReschedulePaymentInstallmentCommand>
{
    public async Task<Result> Handle(ReschedulePaymentInstallmentCommand command, CancellationToken cancellationToken)
    {
        PaymentInstallment? installment = await repository.GetByIdAsync(command.PaymentInstallmentId, cancellationToken);
        if (installment is null || installment.OrganizationId != command.OrganizationId)
            return Result.Failure(PaymentInstallmentErrors.NotFound);

        Result result = installment.Reschedule(command.NewDueDate, command.Reason, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        installment.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class CancelPaymentInstallmentCommandHandler(
    IPaymentInstallmentRepository repository,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CancelPaymentInstallmentCommand>
{
    public async Task<Result> Handle(CancelPaymentInstallmentCommand command, CancellationToken cancellationToken)
    {
        PaymentInstallment? installment = await repository.GetByIdAsync(command.PaymentInstallmentId, cancellationToken);
        if (installment is null || installment.OrganizationId != command.OrganizationId)
            return Result.Failure(PaymentInstallmentErrors.NotFound);

        Result result = installment.Cancel(command.Reason, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        installment.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}

internal sealed class WaivePaymentInstallmentCommandHandler(
    IPaymentInstallmentRepository repository,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<WaivePaymentInstallmentCommand>
{
    public async Task<Result> Handle(WaivePaymentInstallmentCommand command, CancellationToken cancellationToken)
    {
        PaymentInstallment? installment = await repository.GetByIdAsync(command.PaymentInstallmentId, cancellationToken);
        if (installment is null || installment.OrganizationId != command.OrganizationId)
            return Result.Failure(PaymentInstallmentErrors.NotFound);

        Result result = installment.Waive(command.Reason, command.ActorUserId, clock.UtcNow);
        if (result.IsFailure) return result;
        installment.SetModifiedAudit(clock.UtcNow, command.ActorUserId);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success();
    }
}
