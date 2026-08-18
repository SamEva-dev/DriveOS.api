using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.TrainingCredits;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.TrainingCredits.Create;

public sealed record CreateTrainingCreditAccountCommand(
    OrganizationId OrganizationId,
    BillingAccountId BillingAccountId,
    string CreditType,
    DateOnly? ExpirationDate,
    UserId ActorUserId) : ICommand<TrainingCreditAccountId>;

internal sealed class CreateTrainingCreditAccountCommandValidator : AbstractValidator<CreateTrainingCreditAccountCommand>
{
    public CreateTrainingCreditAccountCommandValidator()
    {
        RuleFor(x => x.OrganizationId).Must(x => !x.IsEmpty);
        RuleFor(x => x.BillingAccountId).Must(x => !x.IsEmpty);
        RuleFor(x => x.CreditType).NotEmpty().MaximumLength(80);
        RuleFor(x => x.ActorUserId).Must(x => !x.IsEmpty);
    }
}

internal sealed class CreateTrainingCreditAccountCommandHandler(
    IStudentBillingAccountRepository billingAccounts,
    ITrainingCreditAccountRepository creditAccounts,
    IFundingBillingUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateTrainingCreditAccountCommand, TrainingCreditAccountId>
{
    public async Task<Result<TrainingCreditAccountId>> Handle(CreateTrainingCreditAccountCommand command, CancellationToken cancellationToken)
    {
        BillingAccount? billingAccount = await billingAccounts.GetByIdAsync(command.BillingAccountId, cancellationToken);
        if (billingAccount is null || billingAccount.OrganizationId != command.OrganizationId)
            return Result.Failure<TrainingCreditAccountId>(TrainingCreditAccountErrors.BillingAccountNotFound);

        string normalizedCreditType = command.CreditType.Trim().ToUpperInvariant();
        if (await creditAccounts.ExistsAsync(billingAccount.Id, normalizedCreditType, command.ExpirationDate, cancellationToken))
            return Result.Failure<TrainingCreditAccountId>(TrainingCreditAccountErrors.Duplicate);

        DateTimeOffset now = clock.UtcNow;
        Result<TrainingCreditAccount> created = TrainingCreditAccount.Create(
            TrainingCreditAccountId.New(),
            command.OrganizationId,
            billingAccount.Id,
            normalizedCreditType,
            command.ExpirationDate,
            DateOnly.FromDateTime(now.UtcDateTime));

        if (created.IsFailure)
            return Result.Failure<TrainingCreditAccountId>(created.Error);

        created.Value.SetCreatedAudit(now, command.ActorUserId);
        await creditAccounts.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}
