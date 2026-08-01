using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Managers.GetCurrentBranchManager;

internal sealed class
    GetCurrentBranchManagerQueryHandler(
        IBranchManagerReadService
            readService,
        IClock clock)
    : IQueryHandler<
        GetCurrentBranchManagerQuery,
        BranchManagerAssignmentItem>
{
    public async Task<
        Result<
            BranchManagerAssignmentItem>>
        Handle(
            GetCurrentBranchManagerQuery query,
            CancellationToken cancellationToken)
    {
        bool branchExists =
            await readService
                .BranchExistsAsync(
                    query.OrganizationId,
                    query.BranchId,
                    cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<
                BranchManagerAssignmentItem>(
                    BranchErrors.NotFound);
        }

        BranchManagerAssignmentItem?
            manager =
                await readService
                    .GetCurrentAsync(
                        query.OrganizationId,
                        query.BranchId,
                        clock.UtcNow,
                        cancellationToken);

        return manager is null
            ? Result.Failure<
                BranchManagerAssignmentItem>(
                    BranchErrors
                        .CurrentManagerNotFound)
            : Result.Success(manager);
    }
}