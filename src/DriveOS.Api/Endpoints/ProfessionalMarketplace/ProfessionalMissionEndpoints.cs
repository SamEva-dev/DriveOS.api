using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Application.Missions;
using DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalMissionEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalMissionEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Missions");

        g.MapGet("/me/missions",ListCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Read");
        g.MapGet("/me/missions/{missionId:guid}",GetCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Read");
        g.MapPost("/me/missions/{missionId:guid}/accept",AcceptCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Accept");
        g.MapPost("/me/missions/{missionId:guid}/decline",DeclineCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Decline");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/missions",Create)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Create");

        g.MapGet("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/missions",List)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Read");

        g.MapGet("/organizations/{organizationId:guid}/missions/{missionId:guid}",GetForOrganization)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Read");

        g.MapGet("/profiles/{profileId:guid}/missions/{missionId:guid}",GetForProfessional)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Read");

        g.MapPut("/organizations/{organizationId:guid}/missions/{missionId:guid}",Update)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Update");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/propose",Propose)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Propose");

        g.MapPost("/profiles/{profileId:guid}/missions/{missionId:guid}/accept",Accept)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Accept");

        g.MapPost("/profiles/{profileId:guid}/missions/{missionId:guid}/decline",Decline)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Decline");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/activate",Activate)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Activate");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/pause",Pause)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Pause");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/resume",Resume)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Update");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/complete",Complete)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Complete");

        g.MapPost("/organizations/{organizationId:guid}/missions/{missionId:guid}/cancel",Cancel)
            .RequireAuthorization("ProfessionalMarketplace.Missions.Cancel");

        return app;
    }

    private static async Task<IResult> ListCurrent(IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var result=await mediator.Send(new ListCurrentProfessionalMissionsQuery(userId),ct);
        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> GetCurrent(Guid missionId,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var result=await mediator.Send(new GetCurrentProfessionalMissionQuery(new(missionId),userId),ct);
        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> AcceptCurrent(Guid missionId,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var profile=await mediator.Send(new GetCurrentProfessionalProfileQuery(userId),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var result=await mediator.Send(new AcceptProfessionalMissionCommand(new(missionId),new ProfessionalProfileId(profile.Value.Id),userId),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static async Task<IResult> DeclineCurrent(Guid missionId,MissionReasonRequest q,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var profile=await mediator.Send(new GetCurrentProfessionalProfileQuery(userId),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var result=await mediator.Send(new DeclineProfessionalMissionCommand(new(missionId),new ProfessionalProfileId(profile.Value.Id),q.Reason,userId),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static async Task<IResult> Create(
        Guid organizationId,
        Guid engagementId,
        CreateProfessionalMissionRequest q,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();

        ProfessionalMissionId id=new(Guid.NewGuid());
        BranchId? branch=q.BranchId is Guid branchId?new BranchId(branchId):null;

        var result=await mediator.Send(new CreateProfessionalMissionCommand(
            id,
            new OrganizationId(organizationId),
            new ProfessionalEngagementId(engagementId),
            branch,
            q.Title,
            q.Description,
            q.StartsOn,
            q.EndsOn,
            q.TeachingCategoryCodes??[],
            q.EstimatedMinutes,
            q.VehicleProvisionMode,
            (q.TimeWindows??[])
                .Select(x=>new MissionTimeWindowInput(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId))
                .ToArray(),
            actor),ct);

        return result.IsSuccess
            ? Results.Created($"/api/professional-marketplace/organizations/{organizationId}/missions/{id.Value}",new{id=id.Value})
            : Problem(result.Error);
    }

    private static async Task<IResult> List(
        Guid organizationId,Guid engagementId,IMediator mediator,CancellationToken ct)
    {
        var result=await mediator.Send(new ListProfessionalMissionsQuery(
            new OrganizationId(organizationId),new ProfessionalEngagementId(engagementId)),ct);
        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> GetForOrganization(
        Guid organizationId,Guid missionId,IMediator mediator,CancellationToken ct)
    {
        var result=await mediator.Send(new GetProfessionalMissionQuery(
            new(missionId),new OrganizationId(organizationId),null),ct);

        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> GetForProfessional(
        Guid profileId,Guid missionId,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var profile=await mediator.Send(new GetCurrentProfessionalProfileQuery(userId),ct);
        if(profile.IsFailure||profile.Value.Id!=profileId)return Results.NotFound();
        var result=await mediator.Send(new GetCurrentProfessionalMissionQuery(new(missionId),userId),ct);
        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> Update(
        Guid organizationId,Guid missionId,UpdateProfessionalMissionRequest q,
        IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();

        var result=await mediator.Send(new UpdateProfessionalMissionCommand(
            new(missionId),new(organizationId),q.Title,q.Description,q.StartsOn,q.EndsOn,
            q.TeachingCategoryCodes??[],q.EstimatedMinutes,q.VehicleProvisionMode,
            (q.TimeWindows??[]).Select(x=>new MissionTimeWindowInput(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)).ToArray(),
            actor),ct);

        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static Task<IResult> Propose(Guid organizationId,Guid missionId,IMediator mediator,ICurrentUser user,CancellationToken ct)=>
        Mutate(user,a=>new ProposeProfessionalMissionCommand(new(missionId),new(organizationId),a),mediator,ct);

    private static Task<IResult> Accept(Guid profileId,Guid missionId,IMediator mediator,ICurrentUser user,CancellationToken ct)=>
        Mutate(user,a=>new AcceptProfessionalMissionCommand(new(missionId),new(profileId),a),mediator,ct);

    private static async Task<IResult> Decline(Guid profileId,Guid missionId,MissionReasonRequest q,IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var result=await mediator.Send(new DeclineProfessionalMissionCommand(new(missionId),new(profileId),q.Reason,actor),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static Task<IResult> Activate(Guid organizationId,Guid missionId,IMediator mediator,ICurrentUser user,CancellationToken ct)=>
        Mutate(user,a=>new ActivateProfessionalMissionCommand(new(missionId),new(organizationId),a),mediator,ct);

    private static async Task<IResult> Pause(Guid organizationId,Guid missionId,MissionReasonRequest q,IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var result=await mediator.Send(new PauseProfessionalMissionCommand(new(missionId),new(organizationId),q.Reason??string.Empty,actor),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static Task<IResult> Resume(Guid organizationId,Guid missionId,IMediator mediator,ICurrentUser user,CancellationToken ct)=>
        Mutate(user,a=>new ResumeProfessionalMissionCommand(new(missionId),new(organizationId),a),mediator,ct);

    private static Task<IResult> Complete(Guid organizationId,Guid missionId,IMediator mediator,ICurrentUser user,CancellationToken ct)=>
        Mutate(user,a=>new CompleteProfessionalMissionCommand(new(missionId),new(organizationId),a),mediator,ct);

    private static async Task<IResult> Cancel(Guid organizationId,Guid missionId,MissionReasonRequest q,IMediator mediator,ICurrentUser user,CancellationToken ct)
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        var result=await mediator.Send(new CancelProfessionalMissionCommand(new(missionId),new(organizationId),q.Reason??string.Empty,actor),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static async Task<IResult> Mutate<T>(
        ICurrentUser user,
        Func<UserId,T> factory,
        IMediator mediator,
        CancellationToken ct) where T:ICommand
    {
        if(user.UserId is not{} actor)return Results.Unauthorized();
        Result result=await mediator.Send(factory(actor),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record ProfessionalMissionWindowRequest(
    DayOfWeek DayOfWeek,TimeOnly StartTime,TimeOnly EndTime,string TimeZoneId);

internal sealed record CreateProfessionalMissionRequest(
    Guid? BranchId,
    string Title,
    string? Description,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[]? TeachingCategoryCodes,
    int? EstimatedMinutes,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    ProfessionalMissionWindowRequest[]? TimeWindows);

internal sealed record UpdateProfessionalMissionRequest(
    string Title,
    string? Description,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string[]? TeachingCategoryCodes,
    int? EstimatedMinutes,
    ProfessionalVehicleProvisionMode VehicleProvisionMode,
    ProfessionalMissionWindowRequest[]? TimeWindows);

internal sealed record MissionReasonRequest(string? Reason);
