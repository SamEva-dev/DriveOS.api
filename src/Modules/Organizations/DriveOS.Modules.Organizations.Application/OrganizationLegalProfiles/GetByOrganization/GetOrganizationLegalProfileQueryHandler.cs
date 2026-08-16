using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.Models;
using DriveOS.Modules.Organizations.Domain.OrganizationLegalProfiles;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.OrganizationLegalProfiles.GetByOrganization;

internal sealed class GetOrganizationLegalProfileQueryHandler(
    IOrganizationLegalProfileReadService readService
) : IQueryHandler<GetOrganizationLegalProfileQuery, OrganizationLegalProfileResponse>
{
    public async Task<Result<OrganizationLegalProfileResponse>> Handle(
        GetOrganizationLegalProfileQuery query,
        CancellationToken cancellationToken
    )
    {
        var profile = await readService.GetByOrganizationIdAsync(
            query.OrganizationId,
            cancellationToken
        );
        return profile is null
            ? Result.Failure<OrganizationLegalProfileResponse>(
                OrganizationLegalProfileErrors.NotFound
            )
            : Result.Success(profile);
    }
}
