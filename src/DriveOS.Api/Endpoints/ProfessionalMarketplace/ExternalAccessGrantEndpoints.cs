using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.AccessGrants;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ExternalAccessGrantEndpoints
{
    internal static IEndpointRouteBuilder MapExternalAccessGrantEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Access Grants");


        g.MapGet("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/access-grants",List)
            .RequireAuthorization("ProfessionalMarketplace.AccessGrants.Read");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/access-grants",Create)
            .RequireAuthorization("ProfessionalMarketplace.AccessGrants.Manage");

        g.MapPost("/organizations/{organizationId:guid}/access-grants/{grantId:guid}/revoke",Revoke)
            .RequireAuthorization("ProfessionalMarketplace.AccessGrants.Revoke");

        g.MapGet("/organizations/{organizationId:guid}/profiles/{profileId:guid}/access-check",Check)
            .RequireAuthorization("ProfessionalMarketplace.AccessGrants.Read");

        return app;
    }


    private static async Task<IResult> List(Guid organizationId,Guid engagementId,IMediator mediator,CancellationToken ct)
    {
        var result=await mediator.Send(new ListExternalAccessGrantsQuery(new(organizationId),new(engagementId)),ct);
        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> Create(Guid organizationId,Guid engagementId,CreateExternalAccessGrantRequest q,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();

        ExternalAccessGrantId id=new(Guid.NewGuid());
        var result=await mediator.Send(new CreateExternalAccessGrantCommand(
            id,new(organizationId),new(engagementId),q.ResourceType,q.ResourceId,q.Permission,q.StartDate,q.EndDate,actor),ct);

        return result.IsSuccess
            ? Results.Created($"/api/professional-marketplace/organizations/{organizationId}/access-grants/{id.Value}",new{id=id.Value})
            : Problem(result.Error);
    }

    private static async Task<IResult> Revoke(Guid organizationId,Guid grantId,RevokeExternalAccessGrantRequest q,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        var result=await mediator.Send(new RevokeExternalAccessGrantCommand(new(grantId),new(organizationId),q.Reason,actor),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static async Task<IResult> Check(Guid organizationId,Guid profileId,ExternalAccessResourceType resourceType,Guid resourceId,string permission,DateOnly? date,IMediator mediator,CancellationToken ct)
    {
        var result=await mediator.Send(new CheckExternalProfessionalAccessQuery(
            new(organizationId),new(profileId),resourceType,resourceId,permission,date??DateOnly.FromDateTime(DateTime.UtcNow)),ct);
        return result.IsSuccess?Results.Ok(new{allowed=result.Value}):Problem(result.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record CreateExternalAccessGrantRequest(
    ExternalAccessResourceType ResourceType,
    Guid ResourceId,
    string Permission,
    DateOnly StartDate,
    DateOnly EndDate);

internal sealed record RevokeExternalAccessGrantRequest(string Reason);
