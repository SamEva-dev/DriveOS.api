using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Manage;

internal sealed class ReschedulePaymentInstallmentCommandValidator : AbstractValidator<ReschedulePaymentInstallmentCommand>
{
    public ReschedulePaymentInstallmentCommandValidator()
    {
        RuleFor(x => x.PaymentInstallmentId.Value).NotEmpty();
        RuleFor(x => x.NewDueDate).NotEqual(default(DateOnly));
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(3).MaximumLength(1000);
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
    }
}

internal sealed class CancelPaymentInstallmentCommandValidator : AbstractValidator<CancelPaymentInstallmentCommand>
{
    public CancelPaymentInstallmentCommandValidator()
    {
        RuleFor(x => x.PaymentInstallmentId.Value).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(3).MaximumLength(1000);
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
    }
}

internal sealed class WaivePaymentInstallmentCommandValidator : AbstractValidator<WaivePaymentInstallmentCommand>
{
    public WaivePaymentInstallmentCommandValidator()
    {
        RuleFor(x => x.PaymentInstallmentId.Value).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MinimumLength(3).MaximumLength(1000);
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
    }
}
