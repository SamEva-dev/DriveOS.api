using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationSettings.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationSettings;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationSettings.GetOrganizationSettings;

public sealed class GetOrganizationSettingsQueryHandler(
    IOrganizationSettingsReadService readService
) : IQueryHandler<GetOrganizationSettingsQuery, OrganizationSettingsResponse>
{
    public async Task<Result<OrganizationSettingsResponse>> Handle(
        GetOrganizationSettingsQuery query,
        CancellationToken cancellationToken
    )
    {
        OrganizationSettingsResponse? response = await readService.GetByOrganizationIdAsync(
            query.OrganizationId,
            cancellationToken
        );

        return response is null
            ? Result.Failure<OrganizationSettingsResponse>(OrganizationSettingsErrors.NotFound)
            : Result.Success(response);
    }
}
