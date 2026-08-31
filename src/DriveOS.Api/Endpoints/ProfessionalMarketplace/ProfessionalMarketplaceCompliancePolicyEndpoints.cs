using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ProfessionalMarketplace.Application.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Application.ProfessionalProfiles;
using DriveOS.Modules.ProfessionalMarketplace.Domain.Compliance;
using DriveOS.Modules.ProfessionalMarketplace.Domain.ProfessionalProfiles;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalMarketplaceCompliancePolicyEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalMarketplaceCompliancePolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").RequireAuthorization();

        g.MapPost("/compliance/requirements",CreateRequirement)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Manage");

        g.MapPost("/profiles/{profileId:guid}/compliance/reevaluate",Reevaluate)
            .RequireAuthorization("ProfessionalMarketplace.Compliance.Verify");

        g.MapPut("/profiles/{profileId:guid}/visibility",ChangeVisibility)
            .RequireAuthorization("ProfessionalMarketplace.Visibility.Manage");

        return app;
    }

    private static async Task<IResult> CreateRequirement(CreateComplianceRequirementRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var id=new ProfessionalComplianceRequirementId(Guid.NewGuid());
        var r=await m.Send(new CreateProfessionalComplianceRequirementCommand(
            id,q.RequirementCode,q.CountryCode,q.ProfessionalType,q.EvidenceKind,q.EvidenceTypeCode,
            q.Mandatory,q.Blocking,q.ApplicableCategoryCodes??[],q.EffectiveFrom,q.EffectiveTo,q.Version,actor),ct);
        return r.IsSuccess?Results.Created($"/api/professional-marketplace/compliance/requirements/{id.Value}",new{id=id.Value}):Problem(r.Error);
    }

    private static async Task<IResult> Reevaluate(Guid profileId,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ReevaluateProfessionalComplianceCommand(new(profileId),actor),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> ChangeVisibility(Guid profileId,ChangeMarketplaceVisibilityRequest q,IMediator m,ICurrentUser u,CancellationToken ct)
    {
        if(u.UserId is not{} actor)return Results.Unauthorized();
        var r=await m.Send(new ChangeMarketplaceVisibilityCommand(new(profileId),q.Visibility,actor),ct);
        return r.IsSuccess?Results.NoContent():Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.MessageKey}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.MessageKey}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.MessageKey})
    };
}

internal sealed record CreateComplianceRequirementRequest(
    string RequirementCode,
    string CountryCode,
    ProfessionalType ProfessionalType,
    ProfessionalEvidenceKind EvidenceKind,
    string EvidenceTypeCode,
    bool Mandatory,
    bool Blocking,
    string[]? ApplicableCategoryCodes,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    int Version);

internal sealed record ChangeMarketplaceVisibilityRequest(MarketplaceVisibility Visibility);
