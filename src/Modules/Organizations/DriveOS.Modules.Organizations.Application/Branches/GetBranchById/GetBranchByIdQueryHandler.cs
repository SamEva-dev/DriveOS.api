using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Branches.GetBranchById;

public sealed class GetBranchByIdQueryHandler(IBranchReadService branchReadService)
    : IQueryHandler<GetBranchByIdQuery, BranchResponse>
{
    public async Task<Result<BranchResponse>> Handle(
        GetBranchByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        BranchResponse? branch = await branchReadService.GetByIdAsync(
            query.OrganizationId,
            query.BranchId,
            cancellationToken
        );

        return branch is null
            ? Result.Failure<BranchResponse>(BranchErrors.NotFound)
            : Result.Success(branch);
    }
}
