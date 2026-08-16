using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Organizations.GetOrganizationById;

public sealed class GetOrganizationByIdQueryHandler
    : IQueryHandler<GetOrganizationByIdQuery, OrganizationResponse>
{
    private readonly IOrganizationReadService _organizationReadService;

    public GetOrganizationByIdQueryHandler(IOrganizationReadService organizationReadService)
    {
        _organizationReadService = organizationReadService;
    }

    public async Task<Result<OrganizationResponse>> Handle(
        GetOrganizationByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        OrganizationResponse? organization = await _organizationReadService.GetByIdAsync(
            query.OrganizationId,
            cancellationToken
        );

        if (organization is null)
        {
            return Result.Failure<OrganizationResponse>(OrganizationErrors.NotFound);
        }

        return Result.Success(organization);
    }
}
