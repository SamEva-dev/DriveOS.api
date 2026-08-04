using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.GetVersions;

public sealed class GetOrganizationConfigurationVersionsQueryHandler(
    IOrganizationConfigurationReadService readService)
    : IQueryHandler<GetOrganizationConfigurationVersionsQuery, IReadOnlyList<OrganizationConfigurationListItemResponse>>
{
    public async Task<Result<IReadOnlyList<OrganizationConfigurationListItemResponse>>> Handle(
        GetOrganizationConfigurationVersionsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<OrganizationConfigurationListItemResponse> versions =
            await readService.GetVersionsAsync(query.OrganizationId, cancellationToken);

        return Result.Success(versions);
    }
}
