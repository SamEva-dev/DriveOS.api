using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.Installments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.Installments.Create;

internal sealed class CreatePaymentScheduleCommandHandler(
    IStudentBillingAccountRepository accounts,
    IPaymentInstallmentRepository installments,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreatePaymentScheduleCommand, IReadOnlyCollection<PaymentInstallmentId>>
{
    public async Task<Result<IReadOnlyCollection<PaymentInstallmentId>>> Handle(CreatePaymentScheduleCommand command, CancellationToken cancellationToken)
    {
        BillingAccount? account = await accounts.GetByIdAsync(command.BillingAccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId)
            return Result.Failure<IReadOnlyCollection<PaymentInstallmentId>>(PaymentInstallmentErrors.BillingAccountNotFound);

        if (account.Status == BillingAccountStatus.Closed)
            return Result.Failure<IReadOnlyCollection<PaymentInstallmentId>>(PaymentInstallmentErrors.BillingAccountClosed);

        if (command.Installments.Count == 0)
            return Result.Failure<IReadOnlyCollection<PaymentInstallmentId>>(PaymentInstallmentErrors.ScheduleEmpty);

        var created = new List<PaymentInstallment>(command.Installments.Count);
        foreach (CreatePaymentInstallmentInput input in command.Installments.OrderBy(x => x.DueDate))
        {
            PersonId? financingPersonId = input.FinancingPersonId.HasValue ? new PersonId(input.FinancingPersonId.Value) : null;
            OrganizationId? financingOrganizationId = input.FinancingOrganizationId.HasValue ? new OrganizationId(input.FinancingOrganizationId.Value) : null;

            Result<PaymentInstallment> result = PaymentInstallment.Create(
                PaymentInstallmentId.New(),
                command.OrganizationId,
                account.Id,
                input.DueDate,
                input.ExpectedAmount,
                account.Currency,
                financingPersonId,
                financingOrganizationId);

            if (result.IsFailure)
                return Result.Failure<IReadOnlyCollection<PaymentInstallmentId>>(result.Error);

            result.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
            created.Add(result.Value);
        }

        await installments.AddRangeAsync(created, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success<IReadOnlyCollection<PaymentInstallmentId>>(created.Select(x => x.Id).ToArray());
    }
}
