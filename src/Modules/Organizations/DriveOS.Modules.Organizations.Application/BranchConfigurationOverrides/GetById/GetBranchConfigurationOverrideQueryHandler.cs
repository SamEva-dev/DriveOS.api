using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.Modules.Organizations.Domain.BranchConfigurationOverrides;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.GetById;

public sealed class GetBranchConfigurationOverrideQueryHandler(
    IBranchConfigurationOverrideReadService readService
) : IQueryHandler<GetBranchConfigurationOverrideQuery, BranchConfigurationOverrideResponse>
{
    public async Task<Result<BranchConfigurationOverrideResponse>> Handle(
        GetBranchConfigurationOverrideQuery query,
        CancellationToken cancellationToken
    )
    {
        var response = await readService.GetByIdAsync(
            query.OrganizationId,
            query.BranchId,
            query.OverrideId,
            cancellationToken
        );
        return response is null
            ? Result.Failure<BranchConfigurationOverrideResponse>(
                BranchConfigurationOverrideErrors.NotFound
            )
            : Result.Success(response);
    }
}
