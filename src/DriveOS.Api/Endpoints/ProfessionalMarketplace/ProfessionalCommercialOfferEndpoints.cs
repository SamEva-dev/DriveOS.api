using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.CommercialOffers;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalCommercialOfferEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalCommercialOfferEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Commercial Offers");
        g.MapGet("/organizations/{organizationId:guid}/profiles/{profileId:guid}/commercial-offers",List).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Read");
        g.MapPost("/organizations/{organizationId:guid}/commercial-offers",Create).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Create");
        g.MapPut("/organizations/{organizationId:guid}/commercial-offers/{offerId:guid}",Revise).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Counter");
        g.MapPost("/organizations/{organizationId:guid}/commercial-offers/{offerId:guid}/send",Send).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Create");
        g.MapPost("/organizations/{organizationId:guid}/commercial-offers/{offerId:guid}/accept",AcceptByOrganization).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Accept");
        g.MapPost("/profiles/{profileId:guid}/commercial-offers/{offerId:guid}/accept",AcceptByProfessional).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Accept");
        g.MapPost("/organizations/{organizationId:guid}/commercial-offers/{offerId:guid}/finalize",FinalizeOffer).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Accept");
        g.MapPost("/organizations/{organizationId:guid}/commercial-offers/{offerId:guid}/cancel",Cancel).RequireAuthorization("ProfessionalMarketplace.CommercialOffers.Withdraw");
        return app;
    }


    private static async Task<IResult> List(
        Guid organizationId,
        Guid profileId,
        Guid? applicationId,
        Guid? proposalId,
        Guid? opportunityId,
        IMediator m,
        CancellationToken ct)
    {
        var r=await m.Send(new ListProfessionalCommercialOffersQuery(
            new(organizationId),
            new(profileId),
            applicationId is Guid a?new ProfessionalApplicationId(a):null,
            proposalId is Guid p?new ProfessionalProposalId(p):null,
            opportunityId is Guid o?new ProfessionalOpportunityId(o):null),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Create(Guid organizationId,CreateCommercialOfferRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalCommercialOfferId(Guid.NewGuid());
        ProfessionalApplicationId? appId=q.ApplicationId is Guid a?new(a):null;
        ProfessionalProposalId? proposalId=q.ProposalId is Guid p?new(p):null;
        ProfessionalOpportunityId? opportunityId=q.OpportunityId is Guid o?new(o):null;
        var r=await m.Send(new CreateProfessionalCommercialOfferCommand(id,new(organizationId),new(q.ProfessionalProfileId),appId,proposalId,opportunityId,q.Terms,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/commercial-offers/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> Revise(Guid organizationId,Guid offerId,ReviseCommercialOfferRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ReviseProfessionalCommercialOfferCommand(new(offerId),new(organizationId),q.Terms,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static Task<IResult> Send(Guid organizationId,Guid offerId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new SendProfessionalCommercialOfferCommand(new(offerId),new(organizationId),a),m,ct);

    private static Task<IResult> AcceptByOrganization(Guid organizationId,Guid offerId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new AcceptCommercialOfferByOrganizationCommand(new(offerId),new(organizationId),a),m,ct);

    private static async Task<IResult> AcceptByProfessional(Guid profileId,Guid offerId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new AcceptCommercialOfferByProfessionalCommand(new(offerId),new(profileId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static Task<IResult> FinalizeOffer(Guid organizationId,Guid offerId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new FinalizeProfessionalCommercialOfferCommand(new(offerId),new(organizationId),a),m,ct);

    private static async Task<IResult> Cancel(Guid organizationId,Guid offerId,CancelCommercialOfferRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new CancelProfessionalCommercialOfferCommand(new(offerId),new(organizationId),q.Reason,actor),ct);
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

internal sealed record CreateCommercialOfferRequest(
    Guid ProfessionalProfileId,
    Guid? ApplicationId,
    Guid? ProposalId,
    Guid? OpportunityId,
    CommercialOfferTerms Terms);

internal sealed record ReviseCommercialOfferRequest(CommercialOfferTerms Terms);
internal sealed record CancelCommercialOfferRequest(string Reason);
