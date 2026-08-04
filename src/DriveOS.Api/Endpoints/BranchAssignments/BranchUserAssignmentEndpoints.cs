using DomainRelay.Abstractions;
using DriveOS.Api.Contracts;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.CreateBranchUserAssignment;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.EndBranchUserAssignment;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.GetBranchUserAssignmentById;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.GetBranchUserAssignments;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.GetUserBranchAssignments;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.Models;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.ReactivateBranchUserAssignment;
using DriveOS.Modules.Organizations.Application
    .BranchAssignments.SuspendBranchUserAssignment;
using DriveOS.Modules.Organizations.Domain
    .BranchAssignments;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Security.Contracts;

namespace DriveOS.Api.Endpoints
    .BranchAssignments;

public static class
    BranchUserAssignmentEndpoints
{
    public static IEndpointRouteBuilder
        MapBranchUserAssignmentEndpoints(
            this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder branchGroup =
            endpoints.MapGroup(
                    "/api/organizations/{organizationId:guid}/branches/{branchId:guid}/assignments")
                .WithTags(
                    "Branch assignments");

        branchGroup.MapPost(
                "/",
                CreateAssignmentAsync)
            .WithName(
                "CreateBranchUserAssignment")
            .WithSummary(
                "Affecter un utilisateur à une agence")
            .Accepts<
                CreateBranchUserAssignmentRequest>(
                "application/json")
            .Produces<
                CreateBranchUserAssignmentResponse>(
                StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status409Conflict)
            .RequireAuthorization(
                DriveOsPermissionCodes.BranchAssignments.Create);

        branchGroup.MapGet(
                "/",
                GetAssignmentsByBranchAsync)
            .WithName(
                "GetBranchUserAssignments")
            .WithSummary(
                "Lister les utilisateurs affectés à une agence")
            .Produces<
                PagedResponse<
                    BranchUserAssignmentResponse>>(
                StatusCodes.Status200OK)
            .RequireAuthorization(
                DriveOsPermissionCodes.BranchAssignments.Read);

        RouteGroupBuilder assignmentGroup =
            endpoints.MapGroup(
                    "/api/organizations/{organizationId:guid}/branch-assignments")
                .WithTags(
                    "Branch assignments");

        assignmentGroup.MapGet(
                "/{assignmentId:guid}",
                GetAssignmentByIdAsync)
            .WithName(
                "GetBranchUserAssignmentById")
            .WithSummary(
                "Obtenir une affectation d’agence")
            .Produces<
                BranchUserAssignmentResponse>(
                StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(
                StatusCodes.Status404NotFound)
            .RequireAuthorization(
                DriveOsPermissionCodes.BranchAssignments.Read);

        assignmentGroup.MapPost(
                "/{assignmentId:guid}/suspend",
                SuspendAssignmentAsync)
            .WithName(
                "SuspendBranchUserAssignment")
            .WithSummary(
                "Suspendre une affectation d’agence")
            .Accepts<
                ChangeBranchUserAssignmentStatusRequest>(
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
                DriveOsPermissionCodes.BranchAssignments.Suspend);

        assignmentGroup.MapPost(
                "/{assignmentId:guid}/reactivate",
                ReactivateAssignmentAsync)
            .WithName(
                "ReactivateBranchUserAssignment")
            .WithSummary(
                "Réactiver une affectation d’agence")
            .Accepts<
                ChangeBranchUserAssignmentStatusRequest>(
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
                DriveOsPermissionCodes.BranchAssignments.Reactivate);

        assignmentGroup.MapPost(
                "/{assignmentId:guid}/end",
                EndAssignmentAsync)
            .WithName(
                "EndBranchUserAssignment")
            .WithSummary(
                "Terminer une affectation d’agence")
            .Accepts<
                ChangeBranchUserAssignmentStatusRequest>(
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
                DriveOsPermissionCodes.BranchAssignments.End);

        RouteGroupBuilder userGroup =
            endpoints.MapGroup(
                    "/api/organizations/{organizationId:guid}/users/{userId:guid}/branch-assignments")
                .WithTags(
                    "Branch assignments");

        userGroup.MapGet(
                "/",
                GetAssignmentsByUserAsync)
            .WithName(
                "GetUserBranchAssignments")
            .WithSummary(
                "Lister les agences affectées à un utilisateur")
            .Produces<
                PagedResponse<
                    BranchUserAssignmentResponse>>(
                StatusCodes.Status200OK)
            .RequireAuthorization(
                DriveOsPermissionCodes.BranchAssignments.Read);

        return endpoints;
    }

    private static async Task<IResult>
        CreateAssignmentAsync(
            Guid organizationId,
            Guid branchId,
            CreateBranchUserAssignmentRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new CreateBranchUserAssignmentCommand(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId),
                new UserId(
                    request.UserId),
                request.Role,
                request.AssignmentType,
                request.PlannedEndAtUtc);

        Result<BranchUserAssignmentId>
            result =
                await mediator.Send(
                    command,
                    cancellationToken);

        if (result.IsFailure)
        {
            return result.Error
                .ToHttpResult(
                    httpContext);
        }

        Guid assignmentId =
            result.Value.Value;

        return Results.Created(
            $"/api/organizations/{organizationId}/branch-assignments/{assignmentId}",
            new CreateBranchUserAssignmentResponse(
                assignmentId));
    }

    private static async Task<IResult>
        GetAssignmentByIdAsync(
            Guid organizationId,
            Guid assignmentId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var query =
            new GetBranchUserAssignmentByIdQuery(
                new OrganizationId(
                    organizationId),
                new BranchUserAssignmentId(
                    assignmentId));

        Result<BranchUserAssignmentItem>
            result =
                await mediator.Send(
                    query,
                    cancellationToken);

        return result.IsFailure
            ? result.Error.ToHttpResult(
                httpContext)
            : Results.Ok(
                MapResponse(
                    result.Value));
    }

    private static async Task<IResult>
        GetAssignmentsByBranchAsync(
            Guid organizationId,
            Guid branchId,
            [AsParameters]
            GetBranchUserAssignmentsRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        Result<BranchUserAssignmentStatus?>
            statusResult =
                ParseOptionalEnum<
                    BranchUserAssignmentStatus>(
                    request.Status,
                    BranchUserAssignmentErrors
                        .InvalidStatus());

        if (statusResult.IsFailure)
        {
            return statusResult.Error
                .ToHttpResult(
                    httpContext);
        }

        Result<BranchAssignmentRole?>
            roleResult =
                ParseOptionalEnum<
                    BranchAssignmentRole>(
                    request.Role,
                    BranchUserAssignmentErrors
                        .InvalidRole);

        if (roleResult.IsFailure)
        {
            return roleResult.Error
                .ToHttpResult(
                    httpContext);
        }

        Result<BranchAssignmentType?>
            typeResult =
                ParseOptionalEnum<
                    BranchAssignmentType>(
                    request.AssignmentType,
                    BranchUserAssignmentErrors
                        .InvalidType);

        if (typeResult.IsFailure)
        {
            return typeResult.Error
                .ToHttpResult(
                    httpContext);
        }

        var query =
            new GetBranchUserAssignmentsQuery(
                new OrganizationId(
                    organizationId),
                new BranchId(
                    branchId),
                request.PageNumber,
                request.PageSize,
                request.Search,
                statusResult.Value,
                roleResult.Value,
                typeResult.Value,
                ParseSortField(
                    request.SortBy),
                ParseSortDirection(
                    request.SortDirection));

        Result<
            PagedResult<
                BranchUserAssignmentItem>>
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
            MapPagedResponse(
                result.Value));
    }

    private static async Task<IResult>
        GetAssignmentsByUserAsync(
            Guid organizationId,
            Guid userId,
            [AsParameters]
            GetUserBranchAssignmentsRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        Result<BranchUserAssignmentStatus?>
            statusResult =
                ParseOptionalEnum<
                    BranchUserAssignmentStatus>(
                    request.Status,
                    BranchUserAssignmentErrors
                        .InvalidStatus());

        if (statusResult.IsFailure)
        {
            return statusResult.Error
                .ToHttpResult(
                    httpContext);
        }

        Result<BranchAssignmentRole?>
            roleResult =
                ParseOptionalEnum<
                    BranchAssignmentRole>(
                    request.Role,
                    BranchUserAssignmentErrors
                        .InvalidRole);

        if (roleResult.IsFailure)
        {
            return roleResult.Error
                .ToHttpResult(
                    httpContext);
        }

        Result<BranchAssignmentType?>
            typeResult =
                ParseOptionalEnum<
                    BranchAssignmentType>(
                    request.AssignmentType,
                    BranchUserAssignmentErrors
                        .InvalidType);

        if (typeResult.IsFailure)
        {
            return typeResult.Error
                .ToHttpResult(
                    httpContext);
        }

        var query =
            new GetUserBranchAssignmentsQuery(
                new OrganizationId(
                    organizationId),
                new UserId(
                    userId),
                request.PageNumber,
                request.PageSize,
                statusResult.Value,
                roleResult.Value,
                typeResult.Value,
                ParseSortField(
                    request.SortBy),
                ParseSortDirection(
                    request.SortDirection));

        Result<
            PagedResult<
                BranchUserAssignmentItem>>
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
            MapPagedResponse(
                result.Value));
    }

    private static async Task<IResult>
        SuspendAssignmentAsync(
            Guid organizationId,
            Guid assignmentId,
            ChangeBranchUserAssignmentStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new SuspendBranchUserAssignmentCommand(
                new OrganizationId(
                    organizationId),
                new BranchUserAssignmentId(
                    assignmentId),
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

    private static async Task<IResult>
        ReactivateAssignmentAsync(
            Guid organizationId,
            Guid assignmentId,
            ChangeBranchUserAssignmentStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new ReactivateBranchUserAssignmentCommand(
                new OrganizationId(
                    organizationId),
                new BranchUserAssignmentId(
                    assignmentId),
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

    private static async Task<IResult>
        EndAssignmentAsync(
            Guid organizationId,
            Guid assignmentId,
            ChangeBranchUserAssignmentStatusRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken)
    {
        var command =
            new EndBranchUserAssignmentCommand(
                new OrganizationId(
                    organizationId),
                new BranchUserAssignmentId(
                    assignmentId),
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

    private static
        PagedResponse<
            BranchUserAssignmentResponse>
        MapPagedResponse(
            PagedResult<
                BranchUserAssignmentItem> page)
    {
        List<BranchUserAssignmentResponse>
            items =
                page.Items
                    .Select(MapResponse)
                    .ToList();

        return new PagedResponse<
            BranchUserAssignmentResponse>(
                items,
                page.PageNumber,
                page.PageSize,
                page.TotalCount,
                page.TotalPages,
                page.HasPreviousPage,
                page.HasNextPage);
    }

    private static
        BranchUserAssignmentResponse
        MapResponse(
            BranchUserAssignmentItem item)
    {
        return new BranchUserAssignmentResponse(
            item.Id,
            item.OrganizationId,
            item.BranchId,
            item.UserId,
            item.Role,
            item.AssignmentType,
            item.Status,
            item.StartsAtUtc,
            item.PlannedEndAtUtc,
            item.EffectiveEndAtUtc,
            item.SuspensionReason,
            item.SuspendedAtUtc,
            item.SuspendedByUserId,
            item.EndReason,
            item.EndedAtUtc,
            item.EndedByUserId,
            item.CreatedAtUtc,
            item.CreatedByUserId,
            item.LastModifiedAtUtc,
            item.LastModifiedByUserId);
    }

    private static
        Result<TEnum?>
        ParseOptionalEnum<TEnum>(
            string? value,
            Error error)
        where TEnum :
            struct,
            Enum
    {
        if (
            string.IsNullOrWhiteSpace(
                value))
        {
            return Result.Success<
                TEnum?>(
                    null);
        }

        return Enum.TryParse<TEnum>(
            value.Trim(),
            ignoreCase: true,
            out TEnum parsedValue) &&
            Enum.IsDefined(parsedValue)
                ? Result.Success<
                    TEnum?>(
                        parsedValue)
                : Result.Failure<
                    TEnum?>(
                        error);
    }

    private static
        BranchUserAssignmentSortField
        ParseSortField(
            string? value)
    {
        return value?
            .Trim()
            .ToLowerInvariant()
            switch
        {
            "userid" or "user" =>
                BranchUserAssignmentSortField
                    .UserId,

            "role" =>
                BranchUserAssignmentSortField
                    .Role,

            "assignmenttype" or "type" =>
                BranchUserAssignmentSortField
                    .AssignmentType,

            "status" =>
                BranchUserAssignmentSortField
                    .Status,

            "createdatutc" or "createdat" =>
                BranchUserAssignmentSortField
                    .CreatedAtUtc,

            _ =>
                BranchUserAssignmentSortField
                    .StartsAtUtc,
        };
    }

    private static SortDirection
        ParseSortDirection(
            string? value)
    {
        return
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
    }
}