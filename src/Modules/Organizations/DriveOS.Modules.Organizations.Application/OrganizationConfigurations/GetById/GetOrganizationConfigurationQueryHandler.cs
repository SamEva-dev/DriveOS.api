using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationConfigurations.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationConfigurations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationConfigurations.GetById;

public sealed class GetOrganizationConfigurationQueryHandler(
    IOrganizationConfigurationReadService readService
) : IQueryHandler<GetOrganizationConfigurationQuery, OrganizationConfigurationResponse>
{
    public async Task<Result<OrganizationConfigurationResponse>> Handle(
        GetOrganizationConfigurationQuery query,
        CancellationToken cancellationToken
    )
    {
        OrganizationConfigurationResponse? response = await readService.GetByIdAsync(
            query.OrganizationId,
            query.ConfigurationId,
            cancellationToken
        );

        return response is null
            ? Result.Failure<OrganizationConfigurationResponse>(
                OrganizationConfigurationErrors.NotFound
            )
            : Result.Success(response);
    }
}
