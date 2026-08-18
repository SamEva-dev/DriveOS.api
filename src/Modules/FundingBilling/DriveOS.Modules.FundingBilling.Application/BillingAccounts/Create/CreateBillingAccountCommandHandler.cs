using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.FundingBilling.Application.Persistence;
using DriveOS.Modules.FundingBilling.Domain.BillingAccounts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.FundingBilling.Application.BillingAccounts.Create;
public sealed class CreateBillingAccountCommandHandler(IBillingAccountStudentGateway students, IStudentBillingAccountRepository accounts, IFundingBillingUnitOfWork unitOfWork, IClock clock) : ICommandHandler<CreateBillingAccountCommand, BillingAccountId>
{
    public async Task<Result<BillingAccountId>> Handle(CreateBillingAccountCommand command, CancellationToken cancellationToken)
    {
        if (!await students.ExistsAsync(command.OrganizationId, command.StudentId, cancellationToken))
            return Result.Failure<BillingAccountId>(CreateBillingAccountErrors.StudentNotFound);
        if (await accounts.GetByStudentAsync(command.OrganizationId, command.StudentId, cancellationToken) is not null)
            return Result.Failure<BillingAccountId>(BillingAccountErrors.AlreadyExists);
        Result<BillingAccount> created = BillingAccount.CreateForStudent(BillingAccountId.New(), command.OrganizationId, command.StudentId, command.Currency);
        if (created.IsFailure) return Result.Failure<BillingAccountId>(created.Error);
        created.Value.SetCreatedAudit(clock.UtcNow, command.ActorUserId);
        await accounts.AddAsync(created.Value, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(created.Value.Id);
    }
}
