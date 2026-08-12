using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CRM.Application.Activities.CreateActivity;
using DriveOS.Modules.CRM.Application.Activities.GetActivities;
using DriveOS.Modules.CRM.Domain.Activities;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Crm;

public static class CrmActivityEndpoints
{
    public static IEndpointRouteBuilder MapCrmActivityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/crm").WithTags("CRM - Activities");
        group.MapGet("/activities", GetRecentAsync).RequireAuthorization("Crm.Activities.Read");
        group.MapGet("/leads/{leadId:guid}/activities", GetByLeadAsync).RequireAuthorization("Crm.Activities.Read");
        group.MapPost("/leads/{leadId:guid}/activities", CreateAsync).RequireAuthorization("Crm.Activities.Create");
        return endpoints;
    }

    private static async Task<IResult> GetRecentAsync(int? limit, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<IReadOnlyList<CrmActivityResponse>> result = await mediator.Send(
            new GetRecentActivitiesQuery(tenant.OrganizationId.Value, limit ?? 200), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetByLeadAsync(Guid leadId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<IReadOnlyList<CrmActivityResponse>> result = await mediator.Send(
            new GetLeadActivitiesQuery(tenant.OrganizationId.Value, new LeadId(leadId)), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateAsync(Guid leadId, CreateCrmActivityRequest request,
        IMediator mediator, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<Guid> result = await mediator.Send(new CreateCrmActivityCommand(
            tenant.OrganizationId.Value, new LeadId(leadId), request.Type,
            request.Direction, request.Subject, request.Details, request.OccurredAtUtc), ct);

        return result.IsFailure
            ? result.Error.ToHttpResult(context)
            : Results.Created($"/api/crm/leads/{leadId}/activities/{result.Value}", new { activityId = result.Value });
    }
}

public sealed record CreateCrmActivityRequest(CrmActivityType Type,
    CrmActivityDirection Direction, string Subject, string? Details,
    DateTimeOffset OccurredAtUtc);
