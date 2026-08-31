using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Invitations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class FreelanceInvitationEndpoints
{
    internal static IEndpointRouteBuilder MapFreelanceInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Invitations");

        g.MapPost("/organizations/{organizationId:guid}/invitations",Create)
            .RequireAuthorization("ProfessionalMarketplace.Invitations.Create");
        g.MapPost("/organizations/{organizationId:guid}/invitations/{invitationId:guid}/send",Send)
            .RequireAuthorization("ProfessionalMarketplace.Invitations.Send");
        g.MapPost("/organizations/{organizationId:guid}/invitations/{invitationId:guid}/cancel",Cancel)
            .RequireAuthorization("ProfessionalMarketplace.Invitations.Cancel");

        g.MapPost("/invitations/open",Open).AllowAnonymous();
        g.MapPost("/invitations/accept",Accept).RequireAuthorization("ProfessionalMarketplace.Invitations.Accept");
        g.MapPost("/invitations/decline",Decline).AllowAnonymous();

        return app;
    }

    private static async Task<IResult> Create(Guid organizationId,CreateFreelanceInvitationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new FreelanceInvitationId(Guid.NewGuid());
        var r=await m.Send(new CreateFreelanceInvitationCommand(id,new(organizationId),
            q.BranchId is Guid b?new BranchId(b):null,
            q.MissionId is Guid mi?new ProfessionalMissionId(mi):null,
            q.ProfessionalProfileId is Guid p?new ProfessionalProfileId(p):null,
            q.InvitedUserId is Guid iu?new UserId(iu):null,
            q.Email,q.Phone,q.Message,q.ExpirationDate,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/invitations/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> Send(Guid organizationId,Guid invitationId,SendFreelanceInvitationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new SendFreelanceInvitationCommand(new(invitationId),new(organizationId),q.PublicBaseUrl,actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Open(OpenFreelanceInvitationRequest q,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new OpenFreelanceInvitationCommand(q.Token),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Accept(AcceptFreelanceInvitationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} userId)return Results.Unauthorized();
        var r=await m.Send(new AcceptFreelanceInvitationCommand(q.Token,userId),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Decline(DeclineFreelanceInvitationRequest q,IMediator m,CancellationToken ct)
    {
        Result r=await m.Send(new DeclineFreelanceInvitationCommand(q.Token,q.Reason),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Cancel(Guid organizationId,Guid invitationId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        Result r=await m.Send(new CancelFreelanceInvitationCommand(new(invitationId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record CreateFreelanceInvitationRequest(
    Guid? BranchId,Guid? MissionId,Guid? ProfessionalProfileId,Guid? InvitedUserId,
    string? Email,string? Phone,string? Message,DateOnly ExpirationDate);
internal sealed record SendFreelanceInvitationRequest(string PublicBaseUrl);
internal sealed record OpenFreelanceInvitationRequest(string Token);
internal sealed record AcceptFreelanceInvitationRequest(string Token);
internal sealed record DeclineFreelanceInvitationRequest(string Token,string? Reason);
