using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Branches.SetPrimaryBranch;

internal sealed class SetPrimaryBranchCommandHandler(
    IBranchRepository branchRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetPrimaryBranchCommand>
{
    public async Task<Result> Handle(
        SetPrimaryBranchCommand command,
        CancellationToken cancellationToken)
    {
        Branch? targetBranch = await branchRepository.GetByIdAsync(
            command.BranchId,
            asNoTracking: false,
            cancellationToken);

        if (targetBranch is null ||
            targetBranch.OrganizationId != command.OrganizationId)
        {
            return Result.Failure(BranchErrors.NotFound);
        }

        Result setPrimaryResult = targetBranch.SetAsPrimary();

        if (setPrimaryResult.IsFailure)
        {
            return setPrimaryResult;
        }

        Branch? currentPrimary = await branchRepository.GetPrimaryAsync(
            command.OrganizationId,
            asNoTracking: false,
            cancellationToken);

        if (currentPrimary is not null &&
            currentPrimary.Id != targetBranch.Id)
        {
            currentPrimary.RemovePrimaryDesignation();
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
