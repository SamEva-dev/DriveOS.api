using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.ProfessionalMarketplace.Application.Dashboard;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.ProfessionalMarketplace;

internal static class MarketplaceDashboardEndpoints
{
    internal static IEndpointRouteBuilder MapMarketplaceDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/professional-marketplace").WithTags("Professional Marketplace - Dashboard");

        g.MapGet("/organizations/{organizationId:guid}/dashboard",GetOrganization)
            .RequireAuthorization("ProfessionalMarketplace.Dashboard.Read");

        g.MapGet("/organizations/{organizationId:guid}/analytics",GetOrganizationAnalytics)
            .RequireAuthorization("ProfessionalMarketplace.Analytics.Read");

        g.MapGet("/me/dashboard",GetCurrentProfessional)
            .RequireAuthorization("ProfessionalMarketplace.Dashboard.Read");

        g.MapGet("/profiles/{profileId:guid}/dashboard",GetProfessional)
            .RequireAuthorization("ProfessionalMarketplace.Dashboard.Read");

        return app;
    }

    private static async Task<IResult> GetOrganization(
        Guid organizationId,DateOnly? from,DateOnly? to,IMediator mediator,CancellationToken ct)
    {
        DateOnly end=to??DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly start=from??end.AddDays(-30);
        var r=await mediator.Send(new GetOrganizationMarketplaceDashboardQuery(new(organizationId),start,end),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetOrganizationAnalytics(
        Guid organizationId,DateOnly? from,DateOnly? to,IMediator mediator,CancellationToken ct)
    {
        DateOnly end=to??DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly start=from??end.AddDays(-30);
        var r=await mediator.Send(new GetOrganizationMarketplaceDashboardQuery(new(organizationId),start,end),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetCurrentProfessional(
        DateOnly? from,DateOnly? to,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        DateOnly end=to??DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly start=from??end.AddDays(-30);
        var r=await mediator.Send(new GetCurrentProfessionalMarketplaceDashboardQuery(userId,start,end),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static async Task<IResult> GetProfessional(
        Guid profileId,DateOnly? from,DateOnly? to,IMediator mediator,ICurrentUser currentUser,CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        DateOnly end=to??DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly start=from??end.AddDays(-30);
        var r=await mediator.Send(new GetCurrentProfessionalMarketplaceDashboardQuery(userId,start,end,new(profileId)),ct);
        return r.IsSuccess?Results.Ok(r.Value):Problem(r.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}
