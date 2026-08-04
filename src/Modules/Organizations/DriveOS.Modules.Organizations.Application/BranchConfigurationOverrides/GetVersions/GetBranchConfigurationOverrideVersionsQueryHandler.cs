using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.Models;
using DriveOS.SharedKernel.Results;
namespace DriveOS.Modules.Organizations.Application.BranchConfigurationOverrides.GetVersions;
public sealed class GetBranchConfigurationOverrideVersionsQueryHandler(IBranchConfigurationOverrideReadService readService)
    : IQueryHandler<GetBranchConfigurationOverrideVersionsQuery, IReadOnlyList<BranchConfigurationOverrideListItemResponse>>
{
    public async Task<Result<IReadOnlyList<BranchConfigurationOverrideListItemResponse>>> Handle(GetBranchConfigurationOverrideVersionsQuery query, CancellationToken cancellationToken)
        => Result.Success(await readService.GetVersionsAsync(query.OrganizationId, query.BranchId, cancellationToken));
}
