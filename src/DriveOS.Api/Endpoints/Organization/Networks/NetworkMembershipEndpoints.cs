using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.Organizations.Application.Networks.AddNetworkMember;
using DriveOS.Modules.Organizations.Application.Networks.RemoveNetworkMember;
using DriveOS.Modules.Organizations.Domain.Networks;
using DriveOS.Modules.Organizations.Domain.Organizations;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Endpoints.Organization.Networks;

public static class NetworkMembershipEndpoints
{
    public static IEndpointRouteBuilder MapNetworkMembershipEndpoints(
        this IEndpointRouteBuilder endpoints
    )
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/networks/current/members")
            .WithTags("Networks - Organizations");
        group
            .MapGet("/", ListAsync)
            .WithName("GetCurrentNetworkMembers")
            .RequireAuthorization(DriveOsPermissionCodes.Networks.Read);
        group
            .MapGet("/candidates", ListCandidatesAsync)
            .WithName("GetCurrentNetworkMemberCandidates")
            .RequireAuthorization(DriveOsPermissionCodes.Networks.Read);
        group
            .MapPost("/", AddAsync)
            .WithName("AddCurrentNetworkMember")
            .Produces<NetworkMemberResponse>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.Networks.Manage);
        group
            .MapDelete("/{memberOrganizationId:guid}", RemoveAsync)
            .WithName("RemoveCurrentNetworkMember")
            .Produces(StatusCodes.Status204NoContent)
            .RequireAuthorization(DriveOsPermissionCodes.Networks.Manage);
        return endpoints;
    }

    private static async Task<IResult> ListAsync(
        ICurrentTenant tenant,
        OrganizationsDbContext db,
        HttpContext context,
        CancellationToken ct
    )
    {
        Result<OrganizationId> networkId = await ResolveCurrentNetworkAsync(tenant, db, ct);
        if (networkId.IsFailure)
            return networkId.Error.ToHttpResult(context);
        NetworkMemberResponse[] members = await (
            from membership in db.NetworkOrganizationMemberships.AsNoTracking()
            join organization in db.Organizations.AsNoTracking()
                on membership.MemberOrganizationId equals organization.Id
            where
                membership.NetworkOrganizationId == networkId.Value && membership.EndedAtUtc == null
            orderby organization.LegalName
            select new NetworkMemberResponse(
                membership.Id.Value,
                organization.Id.Value,
                organization.LegalName,
                organization.CountryCode,
                organization.Status.ToString(),
                membership.JoinedAtUtc
            )
        ).ToArrayAsync(ct);
        return Results.Ok(members);
    }

    private static async Task<IResult> ListCandidatesAsync(
        ICurrentTenant tenant,
        OrganizationsDbContext db,
        HttpContext context,
        CancellationToken ct
    )
    {
        Result<OrganizationId> networkId = await ResolveCurrentNetworkAsync(tenant, db, ct);
        if (networkId.IsFailure)
            return networkId.Error.ToHttpResult(context);
        OrganizationId[] assignedIds = await db
            .NetworkOrganizationMemberships.AsNoTracking()
            .Where(x => x.EndedAtUtc == null)
            .Select(x => x.MemberOrganizationId)
            .ToArrayAsync(ct);
        NetworkMemberCandidateResponse[] candidates = await db
            .Organizations.AsNoTracking()
            .Where(x =>
                x.Type == OrganizationType.DrivingSchool && x.Status != OrganizationStatus.Closed
            )
            .OrderBy(x => x.LegalName)
            .Select(x => new NetworkMemberCandidateResponse(
                x.Id.Value,
                x.LegalName,
                x.CountryCode,
                x.Status.ToString(),
                assignedIds.Contains(x.Id)
            ))
            .ToArrayAsync(ct);
        return Results.Ok(candidates);
    }

    private static async Task<IResult> AddAsync(
        AddNetworkMemberRequest request,
        ICurrentTenant tenant,
        OrganizationsDbContext db,
        IMediator mediator,
        HttpContext context,
        CancellationToken ct
    )
    {
        Result<OrganizationId> networkId = await ResolveCurrentNetworkAsync(tenant, db, ct);
        if (networkId.IsFailure)
            return networkId.Error.ToHttpResult(context);
        var memberId = new OrganizationId(request.MemberOrganizationId);
        Result<NetworkOrganizationMembershipId> result = await mediator.Send(
            new AddNetworkMemberCommand(networkId.Value, memberId),
            ct
        );
        if (result.IsFailure)
            return result.Error.ToHttpResult(context);

        NetworkMemberResponse response = await (
            from membership in db.NetworkOrganizationMemberships.AsNoTracking()
            join organization in db.Organizations.AsNoTracking()
                on membership.MemberOrganizationId equals organization.Id
            where membership.Id == result.Value
            select new NetworkMemberResponse(
                membership.Id.Value,
                organization.Id.Value,
                organization.LegalName,
                organization.CountryCode,
                organization.Status.ToString(),
                membership.JoinedAtUtc
            )
        ).SingleAsync(ct);
        return Results.Created($"/api/networks/current/members/{memberId.Value}", response);
    }

    private static async Task<IResult> RemoveAsync(
        Guid memberOrganizationId,
        ICurrentTenant tenant,
        OrganizationsDbContext db,
        IMediator mediator,
        HttpContext context,
        CancellationToken ct
    )
    {
        Result<OrganizationId> networkId = await ResolveCurrentNetworkAsync(tenant, db, ct);
        if (networkId.IsFailure)
            return networkId.Error.ToHttpResult(context);
        var memberId = new OrganizationId(memberOrganizationId);
        Result result = await mediator.Send(
            new RemoveNetworkMemberCommand(networkId.Value, memberId),
            ct
        );
        if (result.IsFailure)
            return result.Error.ToHttpResult(context);
        return Results.NoContent();
    }

    private static async Task<Result<OrganizationId>> ResolveCurrentNetworkAsync(
        ICurrentTenant tenant,
        OrganizationsDbContext db,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return Result.Failure<OrganizationId>(
                NetworkOrganizationMembershipErrors.CurrentOrganizationMustBeNetwork
            );

        DriveOS.Modules.Organizations.Domain.Organizations.Organization? current = await db
            .Organizations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == tenant.OrganizationId.Value, ct);

        return current?.Type == OrganizationType.DrivingSchoolNetwork
            ? Result.Success(current.Id)
            : Result.Failure<OrganizationId>(
                NetworkOrganizationMembershipErrors.CurrentOrganizationMustBeNetwork
            );
    }
}
