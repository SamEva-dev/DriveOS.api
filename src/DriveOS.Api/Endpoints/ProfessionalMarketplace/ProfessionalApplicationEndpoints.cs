using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Application.Applications;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalApplicationEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalApplicationEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Applications");
        g.MapGet("/organizations/{organizationId:guid}/opportunities/{opportunityId:guid}/applications",ListByOpportunity).RequireAuthorization("ProfessionalMarketplace.Applications.Read");
        g.MapPost("/opportunities/{opportunityId:guid}/applications",Submit).RequireAuthorization("ProfessionalMarketplace.Applications.Create");
        g.MapPost("/organizations/{organizationId:guid}/applications/{applicationId:guid}/review",Review).RequireAuthorization("ProfessionalMarketplace.Applications.Review");
        g.MapPost("/organizations/{organizationId:guid}/applications/{applicationId:guid}/shortlist",Shortlist).RequireAuthorization("ProfessionalMarketplace.Applications.Shortlist");
        g.MapPost("/organizations/{organizationId:guid}/applications/{applicationId:guid}/accept",Accept).RequireAuthorization("ProfessionalMarketplace.Applications.Accept");
        g.MapPost("/organizations/{organizationId:guid}/applications/{applicationId:guid}/reject",Reject).RequireAuthorization("ProfessionalMarketplace.Applications.Reject");
        g.MapPost("/profiles/{profileId:guid}/applications/{applicationId:guid}/withdraw",Withdraw).RequireAuthorization("ProfessionalMarketplace.Applications.Withdraw");
        return app;
    }

    private static async Task<IResult> ListByOpportunity(Guid organizationId,Guid opportunityId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new ListProfessionalApplicationsQuery(new(organizationId),new(opportunityId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Submit(Guid opportunityId,SubmitApplicationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalApplicationId(Guid.NewGuid());
        var r=await m.Send(new SubmitProfessionalApplicationCommand(id,new(opportunityId),new(q.ProfessionalProfileId),q.Message,q.ProposedRate,q.Currency,q.RateUnit,q.Negotiable,q.AvailableFrom,q.AvailableUntil,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/applications/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static Task<IResult> Review(Guid organizationId,Guid applicationId,IMediator m,ICurrentUser u,CancellationToken ct)=>Mutate(u,a=>new ReviewProfessionalApplicationCommand(new(applicationId),new(organizationId),a),m,ct);
    private static Task<IResult> Shortlist(Guid organizationId,Guid applicationId,IMediator m,ICurrentUser u,CancellationToken ct)=>Mutate(u,a=>new ShortlistProfessionalApplicationCommand(new(applicationId),new(organizationId),a),m,ct);
    private static Task<IResult> Accept(Guid organizationId,Guid applicationId,IMediator m,ICurrentUser u,CancellationToken ct)=>Mutate(u,a=>new AcceptProfessionalApplicationCommand(new(applicationId),new(organizationId),a),m,ct);

    private static async Task<IResult> Reject(Guid organizationId,Guid applicationId,RejectApplicationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new RejectProfessionalApplicationCommand(new(applicationId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Withdraw(Guid profileId,Guid applicationId,WithdrawApplicationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new WithdrawProfessionalApplicationCommand(new(applicationId),new(profileId),q.Reason,actor),ct);
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
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.MessageKey}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.MessageKey}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey})
    };
}

internal sealed record SubmitApplicationRequest(Guid ProfessionalProfileId,string Message,decimal? ProposedRate,string? Currency,ProfessionalRateUnit? RateUnit,bool Negotiable,DateOnly? AvailableFrom,DateOnly? AvailableUntil);
internal sealed record RejectApplicationRequest(string Reason);
internal sealed record WithdrawApplicationRequest(string? Reason);
