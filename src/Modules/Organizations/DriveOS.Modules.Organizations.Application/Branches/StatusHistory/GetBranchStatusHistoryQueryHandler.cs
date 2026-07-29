using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application
    .Branches.Models;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application
    .Branches.StatusHistory;

internal sealed class GetBranchStatusHistoryQueryHandler(
    IBranchReadService branchReadService)
    : IQueryHandler<
        GetBranchStatusHistoryQuery,
        IReadOnlyList<
            BranchStatusHistoryItem>>
{
    public async Task<
        Result<
            IReadOnlyList<
                BranchStatusHistoryItem>>> Handle(
        GetBranchStatusHistoryQuery query,
        CancellationToken cancellationToken)
    {
        BranchResponse? branch =
            await branchReadService.GetByIdAsync(
                query.OrganizationId,
                query.BranchId,
                cancellationToken);

        if (branch is null)
        {
            return Result.Failure<
                IReadOnlyList<
                    BranchStatusHistoryItem>>(
                BranchErrors.NotFound);
        }

        IReadOnlyList<
            BranchStatusHistoryItem> history =
            await branchReadService
                .GetStatusHistoryAsync(
                    query.OrganizationId,
                    query.BranchId,
                    cancellationToken);

        return Result.Success(history);
    }
}