using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.FundingPlans;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using FluentValidation;

namespace DriveOS.Modules.FundingBilling.Application.FundingPlans.Create;

public sealed record CreateFundingAllocationInput(Guid? FinancingPersonId, Guid? FinancingOrganizationId, decimal RequestedAmount, string? ExternalReference);
public sealed record CreateFundingPlanCommand(OrganizationId OrganizationId, BillingAccountId BillingAccountId, Guid ContractId, decimal TotalCost, decimal StudentContribution, IReadOnlyCollection<CreateFundingAllocationInput> Allocations, UserId ActorUserId) : ICommand<FundingPlanId>;

internal sealed class CreateFundingPlanCommandValidator : AbstractValidator<CreateFundingPlanCommand>
{
    public CreateFundingPlanCommandValidator()
    {
        RuleFor(x => x.ContractId).NotEmpty(); RuleFor(x => x.TotalCost).GreaterThan(0m); RuleFor(x => x.StudentContribution).GreaterThanOrEqualTo(0m);
        RuleForEach(x => x.Allocations).ChildRules(a => { a.RuleFor(x => x.RequestedAmount).GreaterThan(0m); });
    }
}

internal sealed class CreateFundingPlanCommandHandler(IStudentBillingAccountRepository accounts, IFundingPlanRepository plans, IBillingPartyRepository billingParties, IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CreateFundingPlanCommand, FundingPlanId>
{
    public async Task<Result<FundingPlanId>> Handle(CreateFundingPlanCommand command, CancellationToken cancellationToken)
    {
        BillingAccount? account = await accounts.GetByIdAsync(command.BillingAccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId) return Result.Failure<FundingPlanId>(FundingPlanErrors.BillingAccountNotFound);
        if (account.Status == BillingAccountStatus.Closed) return Result.Failure<FundingPlanId>(FundingPlanErrors.BillingAccountClosed);
        if (await plans.ExistsForContractAsync(command.OrganizationId, command.ContractId, cancellationToken)) return Result.Failure<FundingPlanId>(FundingPlanErrors.AlreadyExistsForContract);
        Result<FundingPlan> created = FundingPlan.Create(FundingPlanId.New(), command.OrganizationId, account.Id, account.StudentId, command.ContractId, command.TotalCost, command.StudentContribution, account.Currency);
        if (created.IsFailure) return Result.Failure<FundingPlanId>(created.Error);
        foreach (CreateFundingAllocationInput input in command.Allocations)
        {
            PersonId? person = input.FinancingPersonId.HasValue ? new PersonId(input.FinancingPersonId.Value) : null; OrganizationId? organization = input.FinancingOrganizationId.HasValue ? new OrganizationId(input.FinancingOrganizationId.Value) : null;
            DateOnly businessDate = DateOnly.FromDateTime(clock.UtcNow.UtcDateTime);
            if (!await billingParties.IsAuthorizedAsync(account.Id, person, organization, BillingPartyRole.Funder, input.RequestedAmount, businessDate, cancellationToken)) return Result.Failure<FundingPlanId>(FundingPlanErrors.FunderNotAuthorized);
            Result<FundingAllocationId> added = created.Value.AddAllocation(FundingAllocationId.New(), person, organization, input.RequestedAmount, input.ExternalReference);
            if (added.IsFailure) return Result.Failure<FundingPlanId>(added.Error);
        }
        created.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId); await plans.AddAsync(created.Value, cancellationToken); await unitOfWork.CommitAsync(cancellationToken); return Result.Success(created.Value.Id);
    }
}
