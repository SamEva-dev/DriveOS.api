using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Branches.Managers.GetBranchManagerHistory;

internal sealed class
    GetBranchManagerHistoryQueryHandler(
        IBranchManagerReadService
            readService)
    : IQueryHandler<
        GetBranchManagerHistoryQuery,
        IReadOnlyList<
            BranchManagerAssignmentItem>>
{
    public async Task<
        Result<
            IReadOnlyList<
                BranchManagerAssignmentItem>>>
        Handle(
            GetBranchManagerHistoryQuery query,
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
                IReadOnlyList<
                    BranchManagerAssignmentItem>>(
                        BranchErrors.NotFound);
        }

        IReadOnlyList<
            BranchManagerAssignmentItem>
            assignments =
                await readService
                    .GetHistoryAsync(
                        query.OrganizationId,
                        query.BranchId,
                        cancellationToken);

        return Result.Success(
            assignments);
    }
}