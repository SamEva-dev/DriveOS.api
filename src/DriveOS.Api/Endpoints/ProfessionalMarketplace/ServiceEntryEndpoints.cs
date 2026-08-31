using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ServiceEntries;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;
internal static class ServiceEntryEndpoints
{
    internal static IEndpointRouteBuilder MapServiceEntryEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Service Entries");
        g.MapGet("/me/service-entries",ListCurrent).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Read");
        g.MapGet("/me/service-entries/{entryId:guid}",GetCurrent).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Read");
        g.MapGet("/me/missions/{missionId:guid}/service-entries",ListCurrentByMission).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Read");
        g.MapPost("/me/service-entries/{entryId:guid}/submit",SubmitCurrent).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Submit");
        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/service-entries",Record).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Record");
        g.MapPost("/profiles/{profileId:guid}/service-entries/{entryId:guid}/submit",Submit).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Submit");
        g.MapPost("/organizations/{organizationId:guid}/service-entries/{entryId:guid}/approve",Approve).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Approve");
        g.MapPost("/organizations/{organizationId:guid}/service-entries/{entryId:guid}/reject",Reject).RequireAuthorization("ProfessionalMarketplace.ServiceEntries.Reject");
        // MKT-033: disputes must be opened through /disputes so the dossier, evidence and audit trail exist.
        return app;
    }
    private static async Task<IResult> Record(Guid organizationId,Guid engagementId,RecordServiceEntryRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ServiceEntryId(Guid.NewGuid());
        ProfessionalMissionId? mission=q.MissionId is Guid mid?new(mid):null;
        var r=await m.Send(new RecordServiceEntryCommand(id,new(organizationId),new(engagementId),mission,q.SourceType,q.SourceId,
            q.ServiceDate,q.ServiceCode,q.QuantityMinutes,q.UnitRate,q.ExpensesAmount,q.IndemnitiesAmount,
            q.DiscountAmount,q.Currency,q.Description,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/service-entries/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }
    private static async Task<IResult> ListCurrent(IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ListCurrentProfessionalServiceEntriesQuery(actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }
    private static async Task<IResult> ListCurrentByMission(Guid missionId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ListCurrentProfessionalServiceEntriesQuery(actor,new(missionId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }
    private static async Task<IResult> GetCurrent(Guid entryId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new GetCurrentProfessionalServiceEntryQuery(actor,new(entryId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }
    private static async Task<IResult> SubmitCurrent(Guid entryId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new SubmitCurrentProfessionalServiceEntryCommand(actor,new(entryId)),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }
    private static async Task<IResult> Submit(Guid profileId,Guid entryId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure||profile.Value.Id!=profileId)return Results.NotFound();
        var r=await m.Send(new SubmitServiceEntryCommand(new(entryId),new(profileId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }
    private static Task<IResult> Approve(Guid organizationId,Guid entryId,IMediator m,ICurrentUser u,CancellationToken ct)=>Mutate(u,a=>new ApproveServiceEntryCommand(new(entryId),new(organizationId),a),m,ct);
    private static async Task<IResult> Reject(Guid organizationId,Guid entryId,ServiceEntryReasonRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new RejectServiceEntryCommand(new(entryId),new(organizationId),q.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> Dispute(Guid organizationId,Guid entryId,ServiceEntryReasonRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {if(u.UserId is not{} actor)return Results.Unauthorized();var r=await m.Send(new DisputeServiceEntryCommand(new(entryId),new(organizationId),q.Reason,actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static async Task<IResult> Mutate<T>(ICurrentUser u,Func<UserId,T> factory,IMediator m,CancellationToken ct) where T:ICommand
    {if(u.UserId is not{} actor)return Results.Unauthorized();Result r=await m.Send(factory(actor),ct);return r.IsSuccess?Results.NoContent():Problem(r.Error);}
    private static IResult Problem(Error e)=>e.Type switch{ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),_=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})};
}
internal sealed record RecordServiceEntryRequest(
    Guid? MissionId,ServiceEntrySourceType SourceType,Guid SourceId,DateOnly ServiceDate,string ServiceCode,
    int QuantityMinutes,decimal UnitRate,decimal ExpensesAmount,decimal IndemnitiesAmount,decimal DiscountAmount,
    string Currency,string Description);
internal sealed record ServiceEntryReasonRequest(string Reason);
