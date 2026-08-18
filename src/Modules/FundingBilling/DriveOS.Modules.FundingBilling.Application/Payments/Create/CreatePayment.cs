using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Payments;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.Payments.Create;

public sealed record CreatePaymentCommand(OrganizationId OrganizationId, BillingAccountId BillingAccountId,
    Guid? PayerPersonId, Guid? PayerOrganizationId, decimal Amount, string PaymentMethod,
    string? ExternalReference, UserId ActorUserId) : ICommand<PaymentId>;

internal sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.BillingAccountId.Value).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m);
        RuleFor(x => x.PaymentMethod).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ActorUserId.Value).NotEmpty();
        RuleFor(x => x).Must(x => x.PayerPersonId.HasValue ^ x.PayerOrganizationId.HasValue)
            .WithMessage("Exactly one payer must be specified.");
    }
}

internal sealed class CreatePaymentCommandHandler(IPaymentRepository payments, IStudentBillingAccountRepository accounts, IBillingPartyRepository billingParties,
    IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CreatePaymentCommand, PaymentId>
{
    public async Task<Result<PaymentId>> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        BillingAccount? account = await accounts.GetByIdAsync(command.BillingAccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<PaymentId>(PaymentErrors.BillingAccountNotFound);
        if (account.Status == BillingAccountStatus.Closed)
            return Result.Failure<PaymentId>(PaymentErrors.BillingAccountClosed);

        PersonId? payerPerson = command.PayerPersonId.HasValue ? new PersonId(command.PayerPersonId.Value) : null;
        OrganizationId? payerOrganization = command.PayerOrganizationId.HasValue ? new OrganizationId(command.PayerOrganizationId.Value) : null;
        bool isStudentSelfPayer = payerPerson.HasValue && payerPerson.Value == account.StudentId;
        DateOnly businessDate = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
        if (!isStudentSelfPayer && !await billingParties.IsAuthorizedAsync(account.Id, payerPerson, payerOrganization, BillingPartyRole.Payer, command.Amount, businessDate, cancellationToken))
            return Result.Failure<PaymentId>(PaymentErrors.PayerNotAuthorized);

        if (!string.IsNullOrWhiteSpace(command.ExternalReference))
        {
            Payment? duplicate = await payments.GetByExternalReferenceAsync(command.OrganizationId, command.ExternalReference.Trim(), cancellationToken);
            if (duplicate is not null)
                return Result.Failure<PaymentId>(PaymentErrors.ExternalReferenceAlreadyUsed);
        }

        Result<Payment> created = Payment.Create(PaymentId.New(), command.OrganizationId, command.BillingAccountId,
            payerPerson,
            payerOrganization,
            command.Amount, account.Currency, command.PaymentMethod, command.ExternalReference);
        if (created.IsFailure) return Result.Failure<PaymentId>(created.Error);

        created.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        await payments.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}
