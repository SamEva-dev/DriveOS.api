using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.Modules.FundingBilling.Domain.BillingParties;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.FundingBilling.Application.BillingParties.Manage;

internal sealed class AddBillingPartyCommandHandler(IStudentBillingAccountRepository accounts, IBillingPartyRepository parties, IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<AddBillingPartyCommand, BillingPartyId>
{
    public async Task<Result<BillingPartyId>> Handle(AddBillingPartyCommand command, CancellationToken cancellationToken)
    {
        BillingAccount? account = await accounts.GetByIdAsync(command.BillingAccountId, cancellationToken);
        if (account is null || account.OrganizationId != command.OrganizationId) return Result.Failure<BillingPartyId>(BillingPartyErrors.BillingAccountNotFound);
        PersonId? person = command.PersonId.HasValue ? new PersonId(command.PersonId.Value) : null;
        OrganizationId? organization = command.PartyOrganizationId.HasValue ? new OrganizationId(command.PartyOrganizationId.Value) : null;
        if (await parties.HasActiveAsync(account.Id, person, organization, command.Role, cancellationToken)) return Result.Failure<BillingPartyId>(BillingPartyErrors.Duplicate);
        Result<BillingParty> created = BillingParty.Create(BillingPartyId.New(), command.OrganizationId, account.Id, person, organization, command.Role, command.MaximumAmount, command.EffectiveFrom, command.EffectiveTo, command.Priority, command.IsPrimary);
        if (created.IsFailure) return Result.Failure<BillingPartyId>(created.Error);
        created.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        await parties.AddAsync(created.Value, cancellationToken); await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}

internal sealed class EndBillingPartyCommandHandler(IBillingPartyRepository parties, IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<EndBillingPartyCommand>
{
    public async Task<Result> Handle(EndBillingPartyCommand command, CancellationToken cancellationToken)
    {
        BillingParty? party = await parties.GetByIdAsync(command.BillingPartyId, cancellationToken);
        if (party is null || party.OrganizationId != command.OrganizationId) return Result.Failure(BillingPartyErrors.NotFound);
        Result result = party.End(command.Reason, command.ActorUserId, clock.UtcNow); if (result.IsFailure) return result;
        party.SetModifiedAudit(clock.UtcNow, command.ActorUserId); await unitOfWork.CommitAsync(cancellationToken); return Result.Success();
    }
}
