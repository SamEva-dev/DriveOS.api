using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Disputes;
using DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Application.ServiceEntries;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Disputes;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ServiceDisputeEndpoints
{
    internal static IEndpointRouteBuilder MapServiceDisputeEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Disputes");

        g.MapPost("/me/service-entries/{entryId:guid}/disputes",OpenCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Open");
        g.MapGet("/me/disputes",ListCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Read");
        g.MapGet("/me/disputes/{disputeId:guid}",GetCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Read");
        g.MapPost("/me/disputes/{disputeId:guid}/messages",AddCurrentMessage)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");
        g.MapPost("/me/disputes/{disputeId:guid}/evidence",AddCurrentEvidence)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");
        g.MapPost("/me/disputes/{disputeId:guid}/wait-for/{party}",WaitCurrentFor)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");
        g.MapPost("/me/disputes/{disputeId:guid}/escalate",EscalateCurrent)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        g.MapPost("/organizations/{organizationId:guid}/service-entries/{entryId:guid}/disputes",OpenBySchool)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Open");

        g.MapPost("/profiles/{profileId:guid}/service-entries/{entryId:guid}/disputes",OpenByFreelance)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Open");

        g.MapGet("/organizations/{organizationId:guid}/disputes",ListForSchool)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Read");

        g.MapGet("/organizations/{organizationId:guid}/disputes/{disputeId:guid}",GetForSchool)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Read");

        g.MapGet("/profiles/{profileId:guid}/disputes",ListForFreelance)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Read");

        g.MapGet("/profiles/{profileId:guid}/disputes/{disputeId:guid}",GetForFreelance)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Read");

        g.MapPost("/organizations/{organizationId:guid}/disputes/{disputeId:guid}/messages",AddSchoolMessage)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        g.MapPost("/profiles/{profileId:guid}/disputes/{disputeId:guid}/messages",AddFreelanceMessage)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        g.MapPost("/organizations/{organizationId:guid}/disputes/{disputeId:guid}/evidence",AddEvidence)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        g.MapPost("/profiles/{profileId:guid}/disputes/{disputeId:guid}/evidence",AddFreelanceEvidence)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        g.MapPost("/organizations/{organizationId:guid}/disputes/{disputeId:guid}/wait-for/{party}",WaitFor)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        g.MapPost("/organizations/{organizationId:guid}/disputes/{disputeId:guid}/resolve",Resolve)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Resolve");

        g.MapPost("/organizations/{organizationId:guid}/disputes/{disputeId:guid}/escalate",Escalate)
            .RequireAuthorization("ProfessionalMarketplace.Disputes.Manage");

        return app;
    }

    private static async Task<IResult> OpenBySchool(Guid organizationId,Guid entryId,OpenServiceDisputeRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ServiceDisputeId(Guid.NewGuid());
        var result=await m.Send(new OpenServiceDisputeCommand(id,new(entryId),new(organizationId),
            ServiceDisputeParty.School,organizationId,null,q.Reason,q.Description,
            (q.Evidence??[]).Select(x=>new ServiceDisputeEvidenceInput(x.DocumentReferenceId,x.Label,x.Note)).ToArray(),actor),ct);
        return result.IsSuccess?Results.Created($"/api/professional-marketplace/organizations/{organizationId}/disputes/{id.Value}",new{id=id.Value}):Problem(result.Error);
    }

    private static async Task<IResult> OpenByFreelance(Guid profileId,Guid entryId,OpenFreelanceServiceDisputeRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var current=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(current.IsFailure||current.Value.Id!=profileId)return Results.NotFound();
        var id=new ServiceDisputeId(Guid.NewGuid());
        var result=await m.Send(new OpenServiceDisputeCommand(id,new(entryId),new(q.ClientOrganizationId),
            ServiceDisputeParty.Freelance,Guid.Empty,new ProfessionalProfileId(profileId),q.Reason,q.Description,
            (q.Evidence??[]).Select(x=>new ServiceDisputeEvidenceInput(x.DocumentReferenceId,x.Label,x.Note)).ToArray(),actor),ct);
        return result.IsSuccess?Results.Created($"/api/professional-marketplace/profiles/{profileId}/disputes/{id.Value}",new{id=id.Value}):Problem(result.Error);
    }

    private static async Task<IResult> OpenCurrent(Guid entryId,OpenServiceDisputeRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var entry=await m.Send(new GetCurrentProfessionalServiceEntryQuery(actor,new(entryId)),ct);
        if(entry.IsFailure)return Problem(entry.Error);
        var id=new ServiceDisputeId(Guid.NewGuid());
        var result=await m.Send(new OpenServiceDisputeCommand(id,new(entryId),new(entry.Value.OrganizationId),
            ServiceDisputeParty.Freelance,Guid.Empty,new ProfessionalProfileId(profile.Value.Id),q.Reason,q.Description,
            (q.Evidence??[]).Select(x=>new ServiceDisputeEvidenceInput(x.DocumentReferenceId,x.Label,x.Note)).ToArray(),actor),ct);
        return result.IsSuccess?Results.Created($"/api/professional-marketplace/me/disputes/{id.Value}",new{id=id.Value}):Problem(result.Error);
    }

    private static async Task<IResult> ListCurrent(IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var r=await m.Send(new ListProfessionalServiceDisputesQuery(new(profile.Value.Id)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetCurrent(Guid disputeId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var r=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profile.Value.Id)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> AddCurrentMessage(Guid disputeId,ServiceDisputeMessageRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var lookup=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profile.Value.Id)),ct);
        if(lookup.IsFailure)return Problem(lookup.Error);
        var r=await m.Send(new AddServiceDisputeMessageCommand(new(disputeId),new OrganizationId(lookup.Value.ClientOrganizationId),ServiceDisputeParty.Freelance,q.Message,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> AddCurrentEvidence(Guid disputeId,ServiceDisputeEvidenceRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var lookup=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profile.Value.Id)),ct);
        if(lookup.IsFailure)return Problem(lookup.Error);
        var r=await m.Send(new AddServiceDisputeEvidenceCommand(new(disputeId),new OrganizationId(lookup.Value.ClientOrganizationId),new(q.DocumentReferenceId,q.Label,q.Note),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> WaitCurrentFor(Guid disputeId,ServiceDisputeParty party,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var lookup=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profile.Value.Id)),ct);
        if(lookup.IsFailure)return Problem(lookup.Error);
        var r=await m.Send(new WaitServiceDisputeForCommand(new(disputeId),new OrganizationId(lookup.Value.ClientOrganizationId),party,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> EscalateCurrent(Guid disputeId,EscalateServiceDisputeRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var profile=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(profile.IsFailure)return Problem(profile.Error);
        var lookup=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profile.Value.Id)),ct);
        if(lookup.IsFailure)return Problem(lookup.Error);
        var r=await m.Send(new EscalateServiceDisputeCommand(new(disputeId),new OrganizationId(lookup.Value.ClientOrganizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> ListForSchool(Guid organizationId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new ListOrganizationServiceDisputesQuery(new(organizationId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> ListForFreelance(Guid profileId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var current=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(current.IsFailure||current.Value.Id!=profileId)return Results.NotFound();
        var r=await m.Send(new ListProfessionalServiceDisputesQuery(new(profileId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetForSchool(Guid organizationId,Guid disputeId,IMediator m,CancellationToken ct)
    {
        var r=await m.Send(new GetServiceDisputeQuery(new(disputeId),new OrganizationId(organizationId),null),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetForFreelance(Guid profileId,Guid disputeId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var current=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(current.IsFailure||current.Value.Id!=profileId)return Results.NotFound();
        var r=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profileId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> AddSchoolMessage(Guid organizationId,Guid disputeId,ServiceDisputeMessageRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new AddServiceDisputeMessageCommand(new(disputeId),new(organizationId),ServiceDisputeParty.School,q.Message,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> AddFreelanceMessage(Guid profileId,Guid disputeId,ServiceDisputeMessageRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var current=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(current.IsFailure||current.Value.Id!=profileId)return Results.NotFound();
        var lookup=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profileId)),ct);
        if(lookup.IsFailure)return Problem(lookup.Error);
        var r=await m.Send(new AddServiceDisputeMessageCommand(new(disputeId),new OrganizationId(lookup.Value.ClientOrganizationId),ServiceDisputeParty.Freelance,q.Message,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> AddEvidence(Guid organizationId,Guid disputeId,ServiceDisputeEvidenceRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new AddServiceDisputeEvidenceCommand(new(disputeId),new(organizationId),
            new(q.DocumentReferenceId,q.Label,q.Note),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> AddFreelanceEvidence(Guid profileId,Guid disputeId,ServiceDisputeEvidenceRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var current=await m.Send(new GetCurrentProfessionalProfileQuery(actor),ct);
        if(current.IsFailure||current.Value.Id!=profileId)return Results.NotFound();
        var lookup=await m.Send(new GetServiceDisputeQuery(new(disputeId),null,new ProfessionalProfileId(profileId)),ct);
        if(lookup.IsFailure)return Problem(lookup.Error);
        var r=await m.Send(new AddServiceDisputeEvidenceCommand(new(disputeId),new OrganizationId(lookup.Value.ClientOrganizationId),new(q.DocumentReferenceId,q.Label,q.Note),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> WaitFor(Guid organizationId,Guid disputeId,ServiceDisputeParty party,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new WaitServiceDisputeForCommand(new(disputeId),new(organizationId),party,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Resolve(Guid organizationId,Guid disputeId,ResolveServiceDisputeRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ResolveServiceDisputeCommand(new(disputeId),new(organizationId),q.Outcome,q.Resolution,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> Escalate(Guid organizationId,Guid disputeId,EscalateServiceDisputeRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new EscalateServiceDisputeCommand(new(disputeId),new(organizationId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record ServiceDisputeEvidenceRequest(Guid DocumentReferenceId,string Label,string? Note);
internal sealed record OpenServiceDisputeRequest(ServiceDisputeReason Reason,string Description,ServiceDisputeEvidenceRequest[]? Evidence);
internal sealed record OpenFreelanceServiceDisputeRequest(Guid ClientOrganizationId,ServiceDisputeReason Reason,string Description,ServiceDisputeEvidenceRequest[]? Evidence);
internal sealed record ServiceDisputeMessageRequest(string Message);
internal sealed record ResolveServiceDisputeRequest(ServiceDisputeResolutionOutcome Outcome,string Resolution);
internal sealed record EscalateServiceDisputeRequest(string Reason);
