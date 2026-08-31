using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Application.ServiceStatements;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ServiceStatementEndpoints
{
    internal static IEndpointRouteBuilder MapServiceStatementEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Service Statements");


        g.MapGet("/me/service-statements",ListMine)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Read");
        g.MapGet("/me/service-statements/{statementId:guid}",GetMine)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Read");
        g.MapGet("/me/engagements/{engagementId:guid}/service-statements",ListMineByEngagement)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Read");
        g.MapPost("/me/engagements/{engagementId:guid}/service-statements",CreateMine)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Submit");
        g.MapPost("/me/service-statements/{statementId:guid}/submit",SubmitMine)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Submit");

        g.MapGet("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/service-statements",ListForOrganization)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Read");
        g.MapGet("/organizations/{organizationId:guid}/service-statements/{statementId:guid}",GetForOrganization)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Read");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/service-statements",Create)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Manage");

        g.MapPost("/profiles/{profileId:guid}/service-statements/{statementId:guid}/submit",Submit)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Submit");

        g.MapPost("/organizations/{organizationId:guid}/service-statements/{statementId:guid}/review",StartReview)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Manage");

        g.MapPost("/organizations/{organizationId:guid}/service-statements/{statementId:guid}/refresh",Refresh)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Manage");

        g.MapPost("/organizations/{organizationId:guid}/service-statements/{statementId:guid}/approve",Approve)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Approve");

        g.MapPost("/organizations/{organizationId:guid}/service-statements/{statementId:guid}/reject",Reject)
            .RequireAuthorization("ProfessionalMarketplace.ServiceStatements.Reject");

        return app;
    }


    private static async Task<IResult> ListForOrganization(Guid organizationId,Guid engagementId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new ListOrganizationServiceStatementsQuery(new(organizationId),new(engagementId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetForOrganization(Guid organizationId,Guid statementId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new GetOrganizationServiceStatementQuery(new(organizationId),new(statementId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Create(Guid organizationId,Guid engagementId,CreateServiceStatementRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ServiceStatementId(Guid.NewGuid());
        var r=await m.Send(new CreateServiceStatementCommand(id,new(organizationId),new(engagementId),q.PeriodStart,q.PeriodEnd,actor),ct);
        return r.IsSuccess
            ?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/service-statements/{id.Value}",new{id=id.Value})
            :Problem(r.Error);
    }

    private static async Task<IResult> ListMine(IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ListCurrentProfessionalServiceStatementsQuery(actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> ListMineByEngagement(Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ListCurrentProfessionalServiceStatementsQuery(actor,new(engagementId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetMine(Guid statementId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new GetCurrentProfessionalServiceStatementQuery(actor,new(statementId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> CreateMine(Guid engagementId,CreateServiceStatementRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new CreateCurrentProfessionalServiceStatementCommand(actor,new(engagementId),q.PeriodStart,q.PeriodEnd),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/me/service-statements/{r.Value.Value}",new{id=r.Value.Value}):Problem(r.Error);
    }

    private static Task<IResult> SubmitMine(Guid statementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new SubmitCurrentProfessionalServiceStatementCommand(a,new(statementId)),m,ct);

    private static Task<IResult> Submit(Guid profileId,Guid statementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new SubmitCurrentProfessionalServiceStatementCommand(a,new(statementId)),m,ct);

    private static Task<IResult> StartReview(Guid organizationId,Guid statementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new StartServiceStatementReviewCommand(new(statementId),new(organizationId),a),m,ct);

    private static Task<IResult> Refresh(Guid organizationId,Guid statementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new RefreshServiceStatementCommand(new(statementId),new(organizationId),a),m,ct);

    private static Task<IResult> Approve(Guid organizationId,Guid statementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new ApproveServiceStatementCommand(new(statementId),new(organizationId),a),m,ct);

    private static async Task<IResult> Reject(Guid organizationId,Guid statementId,RejectServiceStatementRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new RejectServiceStatementCommand(new(statementId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Mutate<T>(ICurrentUser u,Func<UserId,T> factory,IMediator m,CancellationToken ct) where T:ICommand
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        Result r=await m.Send(factory(actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record CreateServiceStatementRequest(DateOnly PeriodStart,DateOnly PeriodEnd);
internal sealed record RejectServiceStatementRequest(string Reason);
