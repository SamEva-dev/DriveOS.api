using DomainRelay.Abstractions;
using DriveOS.Api.Security;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application.Branches.GetBranches;
using DriveOS.Modules.Organizations.Application.Branches.Models;
using DriveOS.Modules.Organizations.Application.Organizations.GetOrganizations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.AccessManagement;

public static class AccessManagementEndpoints
{
    public static IEndpointRouteBuilder MapAccessManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/access-management")
            .WithTags("Access Management")
            .AddEndpointFilter<AccessManagerMachineTokenEndpointFilter>();

        group.MapGet("/organizations", GetOrganizationsAsync);
        group.MapGet("/organizations/{organizationId:guid}/branches", GetBranchesAsync);
        return endpoints;
    }

    private static async Task<IResult> GetOrganizationsAsync(IMediator mediator, CancellationToken ct)
    {
        var query = new GetOrganizationsQuery(1, PaginationParameters.MaximumPageSize, null, OrganizationSortField.LegalName, SortDirection.Ascending);
        Result<PagedResult<OrganizationListItem>> result = await mediator.Send(query, ct);
        if (result.IsFailure) return Results.BadRequest(new { code = result.Error.Code, message = result.Error.MessageKey });

        return Results.Ok(result.Value.Items.Select(x => new
        {
            id = x.Id.ToString("D"),
            name = x.LegalName,
            code = x.CountryCode,
            type = x.Type,
            usersCount = 0
        }));
    }

    private static async Task<IResult> GetBranchesAsync(Guid organizationId, IMediator mediator, CancellationToken ct)
    {
        if (organizationId == Guid.Empty) return Results.BadRequest();
        var query = new GetBranchesQuery(
            new OrganizationId(organizationId),
            1,
            PaginationParameters.MaximumPageSize,
            null,
            BranchSortField.Name,
            SortDirection.Ascending);

        Result<PagedResult<BranchListItem>> result = await mediator.Send(query, ct);
        if (result.IsFailure) return Results.NotFound();

        return Results.Ok(result.Value.Items.Select(x => new
        {
            id = x.Id,
            name = x.Name,
            code = x.Code,
            status = x.Status,
            isPrimary = x.IsPrimary
        }));
    }
}
