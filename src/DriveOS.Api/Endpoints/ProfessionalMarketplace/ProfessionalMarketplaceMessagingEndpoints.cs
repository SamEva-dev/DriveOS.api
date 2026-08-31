using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CommunicationEngagement.Application.Conversations;
using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.Modules.ProfessionalMarketplace.Application.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalMarketplaceMessagingEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalMarketplaceMessagingEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace/organizations/{organizationId:guid}/messages").WithTags("Professional Marketplace - Messaging");
        g.MapPost("/conversations",EnsureConversation).RequireAuthorization("ProfessionalMarketplace.Messages.Send");
        g.MapPost("/conversations/{conversationId:guid}",SendMessage).RequireAuthorization("ProfessionalMarketplace.Messages.Send");
        g.MapGet("/conversations/{conversationId:guid}",GetThread).RequireAuthorization("ProfessionalMarketplace.Messages.Read");
        g.MapPost("/conversations/{conversationId:guid}/read",MarkRead).RequireAuthorization("ProfessionalMarketplace.Messages.Read");
        return app;
    }

    private static async Task<IResult> EnsureConversation(Guid organizationId,EnsureMarketplaceConversationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new EnsureMarketplaceConversationCommand(new(organizationId),q.ContextType,q.ContextId,q.ProfessionalProfileId is Guid p?new ProfessionalProfileId(p):null,actor),ct);
        return r.IsSuccess?Results.Ok(new{conversationId=r.Value}):Problem(r.Error);
    }

    private static async Task<IResult> SendMessage(Guid organizationId,Guid conversationId,SendMarketplaceMessageRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new SendConversationMessageCommand(new(conversationId),new(organizationId),actor,q.AttachmentDocumentIds??[],q.Body??string.Empty),ct);
        return r.IsSuccess?Results.Ok(new{messageId=r.Value.Value}):Problem(r.Error);
    }

    private static async Task<IResult> GetThread(Guid organizationId,Guid conversationId,int? take,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        // Organization principal authorizes any permitted organization user; individual professional access uses the user principal.
        var r=await m.Send(new GetConversationThreadQuery(new(conversationId),new(organizationId),ConversationParticipantType.Organization,organizationId,take??100),ct);
        if(r.IsFailure)
            r=await m.Send(new GetConversationThreadQuery(new(conversationId),new(organizationId),ConversationParticipantType.User,actor.Value,take??100),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> MarkRead(Guid organizationId,Guid conversationId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var thread=await m.Send(new GetConversationThreadQuery(new(conversationId),new(organizationId),ConversationParticipantType.User,actor.Value,1),ct);
        ConversationParticipantType type=thread.IsSuccess?ConversationParticipantType.User:ConversationParticipantType.Organization;
        Guid principal=thread.IsSuccess?actor.Value:organizationId;
        var r=await m.Send(new MarkConversationReadCommand(new(conversationId),new(organizationId),type,principal,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.MessageKey }),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.MessageKey }),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey })
    };
}

internal sealed record EnsureMarketplaceConversationRequest(MarketplaceConversationContextType ContextType,Guid ContextId,Guid? ProfessionalProfileId);
internal sealed record SendMarketplaceMessageRequest(string? Body,Guid[]? AttachmentDocumentIds);
