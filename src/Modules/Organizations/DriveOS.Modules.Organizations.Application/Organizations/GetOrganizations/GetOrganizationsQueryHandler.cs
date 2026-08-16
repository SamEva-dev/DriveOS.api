using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Modules.Organizations.Application.Abstractions;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;

public sealed class GetOrganizationsQueryHandler
    : IQueryHandler<GetOrganizationsQuery, PagedResult<OrganizationListItem>>
{
    private readonly IOrganizationReadService _organizationReadService;

    public GetOrganizationsQueryHandler(IOrganizationReadService organizationReadService)
    {
        _organizationReadService = organizationReadService;
    }

    public async Task<Result<PagedResult<OrganizationListItem>>> Handle(
        GetOrganizationsQuery query,
        CancellationToken cancellationToken
    )
    {
        PagedResult<OrganizationListItem> result = await _organizationReadService.GetPagedAsync(
            query.PageNumber,
            query.PageSize,
            query.Search,
            query.SortBy,
            query.SortDirection,
            cancellationToken
        );

        return Result.Success(result);
    }
}
