using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Engagements;
using DriveOS.Modules.ProfessionalMarketplace.Application.AccessGrants;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Engagements;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalEngagementEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalEngagementEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Engagements");

        g.MapPost("/organizations/{organizationId:guid}/engagements",Create)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Manage");

        g.MapGet("/organizations/{organizationId:guid}/profiles/{profileId:guid}/engagements",ListByProfile)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Read");

        g.MapGet("/organizations/{organizationId:guid}/engagements/{engagementId:guid}",Get)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Read");

        g.MapPut("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/preparation",MarkPreparation)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Manage");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/prepare-access",PrepareAccess)
            .RequireAuthorization("ProfessionalMarketplace.AccessGrants.Manage");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/prepare-compliance",PrepareCompliance)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Verify");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/prepare-contract",PrepareContract)
            .RequireAuthorization("ProfessionalMarketplace.Contracts.Read");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/prepare-scheduling",PrepareScheduling)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Manage");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/activate",Activate)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Activate");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/suspend",Suspend)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Suspend");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/resume",Resume)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Manage");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/complete",Complete)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Manage");

        g.MapPost("/organizations/{organizationId:guid}/engagements/{engagementId:guid}/terminate",Terminate)
            .RequireAuthorization("ProfessionalMarketplace.Relationships.Terminate");

        return app;
    }


    private static async Task<IResult> ListByProfile(Guid organizationId,Guid profileId,Guid? commercialOfferId,IMediator m,CancellationToken ct)
    {
        ProfessionalCommercialOfferId? offerId=commercialOfferId is Guid id?new ProfessionalCommercialOfferId(id):null;
        var r=await m.Send(new ListProfessionalEngagementsQuery(new(organizationId),new(profileId),offerId),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Get(Guid organizationId,Guid engagementId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new GetProfessionalEngagementQuery(new(organizationId),new(engagementId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Create(Guid organizationId,CreateEngagementRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalEngagementId(Guid.NewGuid());
        BranchId? branch=q.BranchId is Guid b?new BranchId(b):null;
        var r=await m.Send(new CreateProfessionalEngagementCommand(id,new(organizationId),new(q.CommercialOfferId),branch,actor),ct);
        return r.IsSuccess
            ? Results.Created($"/api/professional-marketplace/organizations/{organizationId}/engagements/{id.Value}",new{id=id.Value})
            : Problem(r.Error);
    }

    private static async Task<IResult> MarkPreparation(Guid organizationId,Guid engagementId,EngagementPreparationRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        if(q.Step==EngagementPreparationStep.Scheduling)
            return Results.Conflict(new
            {
                code="ProfessionalMarketplace.Engagements.SchedulingPreparationMustBeValidated",
                messageKey="errors.professionalMarketplace.engagements.schedulingPreparationMustBeValidated"
            });

        if(q.Step==EngagementPreparationStep.Access)
            return Results.Conflict(new
            {
                code="ProfessionalMarketplace.AccessGrants.AccessPreparationMustBeValidated",
                messageKey="errors.professionalMarketplace.accessGrants.accessPreparationMustBeValidated"
            });
        if(q.Step==EngagementPreparationStep.Contract)
            return Results.Conflict(new
            {
                code="ProfessionalMarketplace.Engagements.ContractPreparationMustBeValidated",
                messageKey="errors.professionalMarketplace.engagements.contractPreparationMustBeValidated"
            });
        if(q.Step==EngagementPreparationStep.Compliance)
            return Results.Conflict(new
            {
                code="ProfessionalMarketplace.Engagements.CompliancePreparationMustBeValidated",
                messageKey="errors.professionalMarketplace.engagements.compliancePreparationMustBeValidated"
            });

        var r=await m.Send(new MarkEngagementPreparationCommand(new(engagementId),new(organizationId),q.Step,q.Completed,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }


    private static async Task<IResult> PrepareCompliance(
        Guid organizationId,Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        Result r=await m.Send(new PrepareProfessionalEngagementComplianceCommand(
            new(engagementId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> PrepareAccess(
        Guid organizationId,Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new PrepareProfessionalEngagementAccessCommand(
            new(engagementId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> PrepareContract(
        Guid organizationId,Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new PrepareProfessionalEngagementContractCommand(
            new(engagementId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> PrepareScheduling(
        Guid organizationId,
        Guid engagementId,
        IMediator m,
        ICurrentUser u,
        CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();

        var r=await m.Send(new PrepareProfessionalEngagementSchedulingCommand(
            new(engagementId),
            new(organizationId),
            actor),ct);

        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static Task<IResult> Activate(Guid organizationId,Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new ActivateProfessionalEngagementCommand(new(engagementId),new(organizationId),a),m,ct);

    private static async Task<IResult> Suspend(Guid organizationId,Guid engagementId,EngagementReasonRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new SuspendProfessionalEngagementCommand(new(engagementId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static Task<IResult> Resume(Guid organizationId,Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)=>
        Mutate(u,a=>new ResumeProfessionalEngagementCommand(new(engagementId),new(organizationId),a),m,ct);

    private static async Task<IResult> Complete(Guid organizationId,Guid engagementId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new CompleteProfessionalEngagementCommand(new(engagementId),new(organizationId),actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> Terminate(Guid organizationId,Guid engagementId,EngagementReasonRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new TerminateProfessionalEngagementCommand(new(engagementId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
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

internal sealed record CreateEngagementRequest(Guid CommercialOfferId,Guid? BranchId);
internal sealed record EngagementPreparationRequest(EngagementPreparationStep Step,bool Completed);
internal sealed record EngagementReasonRequest(string Reason);
