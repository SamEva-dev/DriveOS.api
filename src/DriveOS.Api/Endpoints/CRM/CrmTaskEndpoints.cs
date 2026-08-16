using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CRM.Application.Tasks.CloseTask;
using DriveOS.Modules.CRM.Application.Tasks.CreateTask;
using DriveOS.Modules.CRM.Application.Tasks.GetLeadTasks;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Tasks;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Crm;

public static class CrmTaskEndpoints
{
    public static IEndpointRouteBuilder MapCrmTaskEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/crm").WithTags("CRM - Tasks");
        group.MapGet("/leads/{leadId:guid}/tasks", GetAsync).RequireAuthorization("Crm.Tasks.Read");
        group.MapGet("/tasks", GetPendingAsync).RequireAuthorization("Crm.Tasks.Read");
        group
            .MapPost("/leads/{leadId:guid}/tasks", CreateAsync)
            .RequireAuthorization("Crm.Tasks.Create");
        group
            .MapPost("/tasks/{taskId:guid}/complete", CompleteAsync)
            .RequireAuthorization("Crm.Tasks.Complete");
        group
            .MapPost("/tasks/{taskId:guid}/cancel", CancelAsync)
            .RequireAuthorization("Crm.Tasks.Cancel");
        return endpoints;
    }

    private static async Task<IResult> GetPendingAsync(
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<IReadOnlyList<CrmTaskResponse>> result = await mediator.Send(
            new GetPendingTasksQuery(tenant.OrganizationId.Value),
            ct
        );
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetAsync(
        Guid leadId,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<IReadOnlyList<CrmTaskResponse>> result = await mediator.Send(
            new GetLeadTasksQuery(tenant.OrganizationId.Value, new LeadId(leadId)),
            ct
        );
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateAsync(
        Guid leadId,
        CreateCrmTaskRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<Guid> result = await mediator.Send(
            new CreateCrmTaskCommand(
                tenant.OrganizationId.Value,
                new LeadId(leadId),
                request.Type,
                request.Title,
                request.Notes,
                request.DueAtUtc,
                request.AssignedToUserId is null ? null : new UserId(request.AssignedToUserId.Value)
            ),
            ct
        );
        return result.IsFailure
            ? result.Error.ToHttpResult(context)
            : Results.Created($"/api/crm/tasks/{result.Value}", new { taskId = result.Value });
    }

    private static Task<IResult> CompleteAsync(
        Guid taskId,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    ) => CloseAsync(taskId, false, mediator, tenant, context, ct);

    private static Task<IResult> CancelAsync(
        Guid taskId,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    ) => CloseAsync(taskId, true, mediator, tenant, context, ct);

    private static async Task<IResult> CloseAsync(
        Guid taskId,
        bool cancel,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await mediator.Send(
            new CloseCrmTaskCommand(tenant.OrganizationId.Value, new CrmTaskId(taskId), cancel),
            ct
        );
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }
}

public sealed record CreateCrmTaskRequest(
    CrmTaskType Type,
    string Title,
    string? Notes,
    DateTimeOffset DueAtUtc,
    Guid? AssignedToUserId
);
