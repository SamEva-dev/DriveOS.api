using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Application.Opportunities;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalOpportunityEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalOpportunityEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace/organizations/{organizationId:guid}/opportunities")
            .WithTags("Professional Marketplace - Opportunities");

        g.MapPost("/",Create).RequireAuthorization("ProfessionalMarketplace.Opportunities.Create");
        g.MapGet("/",List).RequireAuthorization("ProfessionalMarketplace.Opportunities.Read");
        g.MapGet("/{opportunityId:guid}",Get).RequireAuthorization("ProfessionalMarketplace.Opportunities.Read");
        g.MapPost("/{opportunityId:guid}/publish",Publish).RequireAuthorization("ProfessionalMarketplace.Opportunities.Publish");
        g.MapPost("/{opportunityId:guid}/pause",Pause).RequireAuthorization("ProfessionalMarketplace.Opportunities.Pause");
        g.MapPost("/{opportunityId:guid}/fill",Fill).RequireAuthorization("ProfessionalMarketplace.Opportunities.Close");
        g.MapPost("/{opportunityId:guid}/cancel",Cancel).RequireAuthorization("ProfessionalMarketplace.Opportunities.Cancel");
        return app;
    }

    private static async Task<IResult> Create(Guid organizationId,CreateOpportunityRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalOpportunityId(Guid.NewGuid());
        BranchId? branch=q.BranchId is Guid b?new BranchId(b):null;
        var r=await m.Send(new CreateProfessionalOpportunityCommand(
            id,new OrganizationId(organizationId),branch,q.Title,q.Description,q.ProfessionalType,
            q.TeachingCategoryCodes??[],q.RequiredLanguageCodes??[],q.RequiredSpecializationCodes??[],
            q.CountryCode,q.AreaCode,q.AreaDisplayName,q.Latitude,q.Longitude,q.RadiusKm,q.StartsOn,q.EndsOn,
            (q.TimeWindows??[]).Select(x=>new OpportunityTimeWindowInput(x.DayOfWeek,x.StartTime,x.EndTime,x.TimeZoneId)).ToArray(),
            q.EstimatedMinutes,q.EngagementType,q.VehicleProvisionMode,q.BudgetMin,q.BudgetMax,q.Currency,q.BudgetUnit,
            q.BudgetNegotiable,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/opportunities/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> List(Guid organizationId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new ListProfessionalOpportunitiesQuery(new OrganizationId(organizationId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Get(Guid organizationId,Guid opportunityId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new GetProfessionalOpportunityQuery(new(opportunityId),new OrganizationId(organizationId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static Task<IResult> Publish(Guid organizationId,Guid opportunityId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,actor=>new PublishProfessionalOpportunityCommand(new(opportunityId),new(organizationId),actor),m,ct);
    private static Task<IResult> Pause(Guid organizationId,Guid opportunityId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,actor=>new PauseProfessionalOpportunityCommand(new(opportunityId),new(organizationId),actor),m,ct);
    private static Task<IResult> Fill(Guid organizationId,Guid opportunityId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,actor=>new FillProfessionalOpportunityCommand(new(opportunityId),new(organizationId),actor),m,ct);

    private static async Task<IResult> Cancel(Guid organizationId,Guid opportunityId,CancelOpportunityRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new CancelProfessionalOpportunityCommand(new(opportunityId),new(organizationId),q.Reason,actor),ct);
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

internal sealed record OpportunityTimeWindowRequest(DayOfWeek DayOfWeek,TimeOnly StartTime,TimeOnly EndTime,string TimeZoneId);
internal sealed record CreateOpportunityRequest(
    Guid? BranchId,string Title,string Description,ProfessionalType ProfessionalType,
    string[]? TeachingCategoryCodes,string[]? RequiredLanguageCodes,string[]? RequiredSpecializationCodes,
    string CountryCode,string? AreaCode,string? AreaDisplayName,decimal? Latitude,decimal? Longitude,int? RadiusKm,
    DateOnly StartsOn,DateOnly EndsOn,OpportunityTimeWindowRequest[]? TimeWindows,int? EstimatedMinutes,
    ProfessionalEngagementType EngagementType,ProfessionalVehicleProvisionMode VehicleProvisionMode,
    decimal? BudgetMin,decimal? BudgetMax,string? Currency,ProfessionalRateUnit? BudgetUnit,bool BudgetNegotiable);
internal sealed record CancelOpportunityRequest(string Reason);
