using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class CompliancePolicyEndpoints
{
    internal static IEndpointRouteBuilder MapCompliancePolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace/compliance")
            .WithTags("Professional Marketplace - Compliance Policies");

        g.MapGet("/policies",ListPolicies)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Read");

        g.MapPost("/policies",CreatePolicy)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Manage");

        g.MapPost("/policies/{policyId:guid}/retire",RetirePolicy)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Manage");

        g.MapGet("/profiles/{profileId:guid}/waivers",ListWaivers)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Read");

        g.MapPost("/profiles/{profileId:guid}/waivers",CreateWaiver)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Override");

        g.MapPost("/profiles/{profileId:guid}/waivers/{waiverId:guid}/revoke",RevokeWaiver)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Override");

        return app;
    }

    private static async Task<IResult> ListPolicies(
        string? countryCode,IMediator mediator,CancellationToken ct)
    {
        var r=await mediator.Send(new GetComplianceCriticalityPoliciesQuery(countryCode),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> CreatePolicy(
        CreateCompliancePolicyRequest q,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();

        var id=new ProfessionalCompliancePolicyId(Guid.NewGuid());
        var r=await mediator.Send(new CreateComplianceCriticalityPolicyCommand(
            id,q.CountryCode,q.RequirementCode,q.Criticality,q.Action,q.GracePeriodDays,
            q.EffectiveFrom,q.EffectiveTo,q.Version,actor),ct);

        return r.IsSuccess
            ?Results.Created($"/api/professional-marketplace/compliance/policies/{id.Value}",new{id=id.Value})
            :Problem(r.Error);
    }

    private static async Task<IResult> RetirePolicy(
        Guid policyId,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        Result r=await mediator.Send(new RetireComplianceCriticalityPolicyCommand(new(policyId),actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static async Task<IResult> ListWaivers(
        Guid profileId,IMediator mediator,CancellationToken ct)
    {
        var r=await mediator.Send(new GetProfessionalComplianceWaiversQuery(new(profileId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> CreateWaiver(
        Guid profileId,
        CreateComplianceWaiverRequest q,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();

        var id=new ProfessionalComplianceWaiverId(Guid.NewGuid());
        var r=await mediator.Send(new CreateProfessionalComplianceWaiverCommand(
            id,new(profileId),q.RequirementCode,q.ValidFrom,q.ValidUntil,q.Reason,actor),ct);

        return r.IsSuccess
            ?Results.Created($"/api/professional-marketplace/compliance/profiles/{profileId}/waivers/{id.Value}",new{id=id.Value})
            :Problem(r.Error);
    }

    private static async Task<IResult> RevokeWaiver(
        Guid profileId,
        Guid waiverId,
        RevokeComplianceWaiverRequest q,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} actor)return Results.Unauthorized();
        Result r=await mediator.Send(new RevokeProfessionalComplianceWaiverCommand(
            new(waiverId),new(profileId),q.Reason,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record CreateCompliancePolicyRequest(
    string CountryCode,
    string RequirementCode,
    ProfessionalComplianceCriticality Criticality,
    ProfessionalComplianceEnforcementAction Action,
    int GracePeriodDays,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int Version);

internal sealed record CreateComplianceWaiverRequest(
    string RequirementCode,
    DateOnly ValidFrom,
    DateOnly ValidUntil,
    string Reason);

internal sealed record RevokeComplianceWaiverRequest(string Reason);
