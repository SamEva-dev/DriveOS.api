using DomainRelay.Abstractions;
using DomainRelay.Mapping.Abstractions.Services;
using DriveOS.Api.Contracts;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application.Branches;
using DriveOS.Modules.Organizations.Application
    .Branches.CreateBranch;
using DriveOS.Modules.Organizations.Application
    .Branches.GetBranchById;
using DriveOS.Modules.Organizations.Application
    .Branches.GetBranches;
using DriveOS.Modules.Organizations.Application
    .Branches.Lifecycle;
using DriveOS.Modules.Organizations.Application.Branches.Managers;
using DriveOS.Modules.Organizations.Application.Branches.Managers.AssignBranchManager;
using DriveOS.Modules.Organizations.Application.Branches.Managers.GetBranchManagerHistory;
using DriveOS.Modules.Organizations.Application.Branches.Managers.GetCurrentBranchManager;
using DriveOS.Modules.Organizations.Application
    .Branches.Models;
using DriveOS.Modules.Organizations.Application
    .Branches.SetPrimaryBranch;
using DriveOS.Modules.Organizations.Application
    .Branches.StatusHistory;
using DriveOS.Modules.Organizations.Application
    .Branches.UpdateBranch;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Endpoints.Organization.Branches;

public static class BranchEndpoints
{
    public static IEndpointRouteBuilder
        MapBranchEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group =
            endpoints.MapGroup(
                    "/api/organizations/{organizationId:guid}/branches")
                .WithTags("Branches");

        group.MapPost(
                "/",
                CreateBranchAsync)
            .WithName("CreateBranch")
            .Produces<CreateBranchResponse>(
                StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Create);

        group.MapGet(
                "/",
                GetBranchesAsync)
            .WithName("GetBranches")
            .Produces<
                PagedResponse<
                    BranchListItemResponse>>(
                StatusCodes.Status200OK)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Read);

        group.MapGet(
                "/{branchId:guid}",
                GetBranchByIdAsync)
            .WithName("GetBranchById")
            .Produces<GetBranchResponse>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Read);

        group.MapPut(
                "/{branchId:guid}",
                UpdateBranchAsync)
            .WithName("UpdateBranch")
            .Accepts<UpdateBranchRequest>(
                "application/json")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Update);

        group.MapPost(
                "/{branchId:guid}/set-primary",
                SetPrimaryBranchAsync)
            .WithName("SetPrimaryBranch")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.SetPrimary);

        group.MapGet(
                "/{branchId:guid}/status-history",
                GetBranchStatusHistoryAsync)
            .WithName(
                "GetBranchStatusHistory")
            .WithSummary(
                "Obtenir l’historique des statuts d’une agence")
            .Produces<
                IReadOnlyList<
                    BranchStatusHistoryItem>>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.StatusHistoryRead);

        group.MapPost(
                "/{branchId:guid}/activate",
                ActivateBranchAsync)
            .WithName("ActivateBranch")
            .Accepts<ChangeBranchStatusRequest>(
                "application/json")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Activate);

        group.MapPost(
                "/{branchId:guid}/restrict",
                RestrictBranchAsync)
            .WithName("RestrictBranch")
            .Accepts<ChangeBranchStatusRequest>(
                "application/json")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Restrict);

        group.MapPost(
                "/{branchId:guid}/suspend",
                SuspendBranchAsync)
            .WithName("SuspendBranch")
            .Accepts<ChangeBranchStatusRequest>(
                "application/json")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Suspend);

        group.MapPost(
                "/{branchId:guid}/reactivate",
                ReactivateBranchAsync)
            .WithName("ReactivateBranch")
            .Accepts<ChangeBranchStatusRequest>(
                "application/json")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Reactivate);

        group.MapPost(
                "/{branchId:guid}/close",
                CloseBranchAsync)
            .WithName("CloseBranch")
            .Accepts<ChangeBranchStatusRequest>(
                "application/json")
            .Produces(
                StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.Branches.Close);

        group.MapPost(
        "/{branchId:guid}/manager",
        AssignBranchManagerAsync)
    .WithName(
        "AssignBranchManager")
    .WithSummary(
        "Affecter le responsable principal d’une agence")
    .Accepts<
        AssignBranchManagerRequest>(
        "application/json")
    .Produces(
        StatusCodes.Status204NoContent)
    .Produces<ApiErrorResponse>(
        StatusCodes.Status400BadRequest)
    .Produces<ApiErrorResponse>(
        StatusCodes.Status404NotFound)
    .Produces<ApiErrorResponse>(
        StatusCodes.Status409Conflict)
    .RequireAuthorization(
        DriveOsPermissionCodes.BranchManagers.Assign);

        group.MapGet(
                "/{branchId:guid}/manager",
                GetCurrentBranchManagerAsync)
            .WithName(
                "GetCurrentBranchManager")
            .WithSummary(
                "Obtenir le responsable principal actuel d’une agence")
            .Produces<
                BranchManagerAssignmentResponse>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .RequireAuthorization(
                DriveOsPermissionCodes.BranchManagers.Read);

        group.MapGet(
                "/{branchId:guid}/manager-history",
                GetBranchManagerHistoryAsync)
            .WithName(
                "GetBranchManagerHistory")
            .WithSummary(
                "Obtenir l’historique des responsables d’une agence")
            .Produces<
                IReadOnlyList<
                    BranchManagerAssignmentResponse>>(
                StatusCodes.Status200OK)
            .RequireAuthorization(
                DriveOsPermissionCodes.BranchManagers.HistoryRead);

        return endpoints;
    }

    private static async Task<IResult>
        CreateBranchAsync(
            Guid organizationId,
            CreateBranchRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new CreateBranchCommand(
                new OrganizationId(
                    organizationId),
                request.Name,
                request.Code,
                request.BranchType,
                request.AddressLine1,
                request.AddressLine2,
                request.PostalCode,
                request.City,
                request.TimeZoneId,
                request.IsPrimary);

        Result<BranchId> result =
            await mediator.Send(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        Guid branchId =
            result.Value.Value;

        return Results.Created(
            $"/api/organizations/{organizationId}/branches/{branchId}",
            new CreateBranchResponse(
                branchId));
    }

    private static async Task<IResult>
        GetBranchByIdAsync(
            Guid organizationId,
            Guid branchId,
            IMediator mediator,
            IObjectMapper mapper,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var query =
            new GetBranchByIdQuery(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId));

        Result<BranchResponse> result =
            await mediator.Send(
                query,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        return Results.Ok(
            mapper.Map<
                BranchResponse,
                GetBranchResponse>(
                result.Value));
    }

    private static async Task<IResult>
        GetBranchesAsync(
            Guid organizationId,
            [AsParameters]
            GetBranchesRequest request,
            IMediator mediator,
            IObjectMapper mapper,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var query =
            new GetBranchesQuery(
                new OrganizationId(
                    organizationId),
                request.PageNumber,
                request.PageSize,
                request.Search,
                ParseSortField(
                    request.SortBy),
                ParseSortDirection(
                    request.SortDirection));

        Result<
            PagedResult<
                BranchListItem>> result =
            await mediator.Send(
                query,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        PagedResult<BranchListItem> page =
            result.Value;

        List<BranchListItemResponse> items =
            mapper.Map<
                List<BranchListItem>,
                List<
                    BranchListItemResponse>>(
                page.Items.ToList());

        return Results.Ok(
            new PagedResponse<
                BranchListItemResponse>(
                items,
                page.PageNumber,
                page.PageSize,
                page.TotalCount,
                page.TotalPages,
                page.HasPreviousPage,
                page.HasNextPage));
    }

    private static async Task<IResult>
        UpdateBranchAsync(
            Guid organizationId,
            Guid branchId,
            UpdateBranchRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new UpdateBranchCommand(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId),
                request.Name,
                request.BranchType,
                request.AddressLine1,
                request.AddressLine2,
                request.PostalCode,
                request.City,
                request.TimeZoneId);

        Result result =
            await mediator.Send(
                command,
                cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult(
                httpContext)
            : Results.NoContent();
    }

    private static async Task<IResult>
        SetPrimaryBranchAsync(
            Guid organizationId,
            Guid branchId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new SetPrimaryBranchCommand(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId));

        Result result =
            await mediator.Send(
                command,
                cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult(
                httpContext)
            : Results.NoContent();
    }

    private static async Task<IResult>
        GetBranchStatusHistoryAsync(
            Guid organizationId,
            Guid branchId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var query =
            new GetBranchStatusHistoryQuery(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId));

        Result<
            IReadOnlyList<
                BranchStatusHistoryItem>> result =
            await mediator.Send(
                query,
                cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult(
                httpContext)
            : Results.Ok(result.Value);
    }

    private static Task<IResult>
        ActivateBranchAsync(
            Guid organizationId,
            Guid branchId,
            ChangeBranchStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        ChangeBranchStatusAsync(
            organizationId,
            branchId,
            BranchStatus.Active,
            request,
            mediator,
            httpContext,
            cancellationToken);

    private static Task<IResult>
        RestrictBranchAsync(
            Guid organizationId,
            Guid branchId,
            ChangeBranchStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        ChangeBranchStatusAsync(
            organizationId,
            branchId,
            BranchStatus.Restricted,
            request,
            mediator,
            httpContext,
            cancellationToken);

    private static Task<IResult>
        SuspendBranchAsync(
            Guid organizationId,
            Guid branchId,
            ChangeBranchStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        ChangeBranchStatusAsync(
            organizationId,
            branchId,
            BranchStatus.Suspended,
            request,
            mediator,
            httpContext,
            cancellationToken);

    private static Task<IResult>
        ReactivateBranchAsync(
            Guid organizationId,
            Guid branchId,
            ChangeBranchStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        ChangeBranchStatusAsync(
            organizationId,
            branchId,
            BranchStatus.Active,
            request,
            mediator,
            httpContext,
            cancellationToken);

    private static Task<IResult>
        CloseBranchAsync(
            Guid organizationId,
            Guid branchId,
            ChangeBranchStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        ChangeBranchStatusAsync(
            organizationId,
            branchId,
            BranchStatus.Closed,
            request,
            mediator,
            httpContext,
            cancellationToken);

    private static async Task<IResult>
        ChangeBranchStatusAsync(
            Guid organizationId,
            Guid branchId,
            BranchStatus targetStatus,
            ChangeBranchStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new ChangeBranchStatusCommand(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId),
                targetStatus,
                request.Reason);

        Result result =
            await mediator.Send(
                command,
                cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult(
                httpContext)
            : Results.NoContent();
    }

    private static BranchSortField
        ParseSortField(
            string? value) =>
        value?.Trim().ToLowerInvariant()
        switch
        {
            "code" =>
                BranchSortField.Code,

            "city" =>
                BranchSortField.City,

            "branchtype" or "type" =>
                BranchSortField.BranchType,

            "status" =>
                BranchSortField.Status,

            "createdatutc" or "createdat" =>
                BranchSortField.CreatedAtUtc,

            _ =>
                BranchSortField.Name,
        };

    private static SortDirection
        ParseSortDirection(
            string? value) =>
        string.Equals(
            value,
            "desc",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            value,
            "descending",
            StringComparison.OrdinalIgnoreCase)
            ? SortDirection.Descending
            : SortDirection.Ascending;

    private static async Task<IResult>
    AssignBranchManagerAsync(
        Guid organizationId,
        Guid branchId,
        AssignBranchManagerRequest request,
        IMediator mediator,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var command =
            new AssignBranchManagerCommand(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId),
                new UserId(
                    request.ManagerUserId));

        Result result =
            await mediator.Send(
                command,
                cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        return Results.NoContent();
    }

    private static async Task<IResult>
        GetCurrentBranchManagerAsync(
            Guid organizationId,
            Guid branchId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var query =
            new GetCurrentBranchManagerQuery(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId));

        Result<
            BranchManagerAssignmentItem>
            result =
                await mediator.Send(
                    query,
                    cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        return Results.Ok(
            MapManagerResponse(
                result.Value));
    }

    private static async Task<IResult>
        GetBranchManagerHistoryAsync(
            Guid organizationId,
            Guid branchId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var query =
            new GetBranchManagerHistoryQuery(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId));

        Result<
            IReadOnlyList<
                BranchManagerAssignmentItem>>
            result =
                await mediator.Send(
                    query,
                    cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        IReadOnlyList<
            BranchManagerAssignmentResponse>
            response =
                result.Value
                    .Select(
                        MapManagerResponse)
                    .ToList();

        return Results.Ok(response);
    }

    private static
        BranchManagerAssignmentResponse
        MapManagerResponse(
            BranchManagerAssignmentItem item)
    {
        return new BranchManagerAssignmentResponse(
            item.Id,
            item.BranchId,
            item.ManagerUserId,
            item.EffectiveFromUtc,
            item.EffectiveToUtc,
            item.Status,
            item.AssignedByUserId,
            item.AssignedAtUtc,
            item.EndedByUserId,
            item.EndedAtUtc);
    }
}