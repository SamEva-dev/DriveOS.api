using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Matching;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class ProfessionalMatchingEndpoints
{
    internal static IEndpointRouteBuilder MapProfessionalMatchingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/professional-marketplace/organizations/{organizationId:guid}/opportunities/{opportunityId:guid}/matches",Match)
            .WithTags("Professional Marketplace - Matching")
            .RequireAuthorization("ProfessionalMarketplace.Matching.Run");
        return app;
    }

    private static async Task<IResult> Match(Guid organizationId,Guid opportunityId,int? limit,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        var r=await mediator.Send(new MatchProfessionalsForOpportunityQuery(
            new ProfessionalOpportunityId(opportunityId),
            new OrganizationId(organizationId),
            limit??20),ct);
        if(!r.IsSuccess)
            return Results.BadRequest(new{code=r.Error.Code,messageKey=r.Error.MessageKey});

        if(currentUser.HasPermission("ProfessionalMarketplace.Matching.Explain"))
            return Results.Ok(r.Value);

        ProfessionalMatchResult[] redacted=r.Value
            .Select(x=>x with
            {
                Breakdown=new ProfessionalMatchBreakdown(0,0,0,0,0,0,0,0),
                Explanations=[]
            })
            .ToArray();

        return Results.Ok(redacted);
    }
}
