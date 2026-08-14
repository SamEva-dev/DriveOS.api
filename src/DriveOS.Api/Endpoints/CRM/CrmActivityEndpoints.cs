using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CRM.Application.Activities.CreateActivity;
using DriveOS.Modules.CRM.Application.Activities.GetActivities;
using DriveOS.Modules.CRM.Application.Activities.ImportActivity;
using DriveOS.Modules.CRM.Application.Activities.Manage;
using DriveOS.Modules.CRM.Application.Activities.Attachments;
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
        group.MapGet("/activities/page", GetPageAsync).RequireAuthorization("Crm.Activities.Read");
        group.MapPost("/activities", CreateUnattachedAsync).RequireAuthorization("Crm.Activities.CreateUnattached");
        group.MapPost("/activities/import", ImportAsync).RequireAuthorization("Crm.Activities.Import");
        group.MapPost("/activities/{activityId:guid}/attach", AttachAsync).RequireAuthorization("Crm.Activities.Attach");
        group.MapPost("/activities/{activityId:guid}/invalidate", InvalidateAsync).RequireAuthorization("Crm.Activities.Invalidate");
        group.MapPost("/activities/{activityId:guid}/sync/retry", RetrySyncAsync).RequireAuthorization("Crm.Activities.Sync.Manage");
        group.MapPost("/activities/{activityId:guid}/sync/abandon", AbandonSyncAsync).RequireAuthorization("Crm.Activities.Sync.Manage");
        group.MapPost("/activities/{activityId:guid}/attachment", UploadAttachmentAsync)
            .DisableAntiforgery().RequireAuthorization("Crm.Activities.Attachments.Upload");
        group.MapGet("/activities/{activityId:guid}/attachment", DownloadAttachmentAsync)
            .RequireAuthorization("Crm.Activities.Attachments.Read");
        group.MapDelete("/activities/{activityId:guid}/attachment", DeleteAttachmentAsync)
            .RequireAuthorization("Crm.Activities.Attachments.Delete");
        group.MapGet("/leads/{leadId:guid}/activities", GetByLeadAsync).RequireAuthorization("Crm.Activities.Read");
        group.MapPost("/leads/{leadId:guid}/activities", CreateAsync).RequireAuthorization("Crm.Activities.Create");
        return endpoints;
    }

    private static async Task<IResult> GetPageAsync(int? pageNumber, int? pageSize, string? search,
        CrmActivityType? type, Guid? advisorUserId, Guid? leadId, bool? unattachedOnly,
        bool? importedOnly, bool? syncErrorsOnly, bool? duplicatesOnly, bool? regularizationOnly,
        bool? unfollowedOnly, DateTimeOffset? fromUtc, DateTimeOffset? toUtc,
        IActivityReadService service, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        var query = new ActivityListQuery(pageNumber ?? 1, pageSize ?? 20, search, type,
            advisorUserId.HasValue ? new UserId(advisorUserId.Value) : null, leadId,
            unattachedOnly ?? false, importedOnly ?? false, syncErrorsOnly ?? false,
            duplicatesOnly ?? false, regularizationOnly ?? false, unfollowedOnly ?? false,
            ReadScope(user), fromUtc, toUtc);
        return Results.Ok(await service.GetPageAsync(tenant.OrganizationId.Value, query, ct));
    }

    private static async Task<IResult> GetRecentAsync(int? limit, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<IReadOnlyList<CrmActivityResponse>> result = await mediator.Send(
            new GetRecentActivitiesQuery(tenant.OrganizationId.Value, limit ?? 200, ReadScope(user)), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetByLeadAsync(Guid leadId, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<IReadOnlyList<CrmActivityResponse>> result = await mediator.Send(
            new GetLeadActivitiesQuery(tenant.OrganizationId.Value, new LeadId(leadId), ReadScope(user)), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateAsync(Guid leadId, CreateCrmActivityRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (request.IsInternal && !user.HasPermission("Crm.Activities.InternalNotes.Create"))
            return Results.Forbid();

        Result<Guid> result = await mediator.Send(new CreateCrmActivityCommand(
            tenant.OrganizationId.Value, new LeadId(leadId), request.Type,
            request.Direction, request.Subject, request.Details, request.OccurredAtUtc,
            request.AdvisorUserId.HasValue ? new UserId(request.AdvisorUserId.Value) : null,
            request.ToMetadata(), request.NextActionTitle, request.NextActionDueAtUtc, request.NextActionType), ct);

        return result.IsFailure
            ? result.Error.ToHttpResult(context)
            : Results.Created($"/api/crm/leads/{leadId}/activities/{result.Value}", new { activityId = result.Value });
    }

    private static Task<IResult> CreateUnattachedAsync(CreateCrmActivityRequest request, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
        CreateCoreAsync(null, request, mediator, tenant, user, context, ct);

    private static Task<IResult> CreateCoreAsync(Guid? leadId, CreateCrmActivityRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct) =>
        CreateWithOptionalLeadAsync(leadId, request, mediator, tenant, user, context, ct);

    private static async Task<IResult> CreateWithOptionalLeadAsync(Guid? leadId, CreateCrmActivityRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (request.IsInternal && !user.HasPermission("Crm.Activities.InternalNotes.Create"))
            return Results.Forbid();
        Result<Guid> result = await mediator.Send(new CreateCrmActivityCommand(tenant.OrganizationId.Value,
            leadId.HasValue ? new LeadId(leadId.Value) : null, request.Type, request.Direction,
            request.Subject, request.Details, request.OccurredAtUtc,
            request.AdvisorUserId.HasValue ? new UserId(request.AdvisorUserId.Value) : null,
            request.ToMetadata(), request.NextActionTitle, request.NextActionDueAtUtc, request.NextActionType), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Created($"/api/crm/activities/{result.Value}", new { activityId = result.Value });
    }

    private static async Task<IResult> AttachAsync(Guid activityId, AttachActivityRequest request,
        IActivityManagementService service, ICurrentTenant tenant, HttpContext context, CancellationToken ct) =>
        await Manage(activityId, service, tenant, context, (org, id) => service.AttachAsync(org, new CrmActivityId(id), request.LeadId, ct));

    private static async Task<IResult> ImportAsync(ImportCrmActivityRequest request, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<ImportCrmActivityResult> result = await mediator.Send(new ImportCrmActivityCommand(
            tenant.OrganizationId.Value, request.LeadId.HasValue ? new LeadId(request.LeadId.Value) : null,
            request.Type, request.Direction, request.Subject, request.Details, request.OccurredAtUtc,
            request.AdvisorUserId.HasValue ? new UserId(request.AdvisorUserId.Value) : null,
            request.ExternalId, request.IdempotencyKey, request.SyncStatus, request.SyncErrorKey,
            request.Result, request.DurationMinutes, request.RequiresRegularization,
            request.AttachmentName, request.AttachmentReference), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        return result.Value.AlreadyImported
            ? Results.Ok(result.Value)
            : Results.Created($"/api/crm/activities/{result.Value.ActivityId}", result.Value);
    }
    private static async Task<IResult> InvalidateAsync(Guid activityId, InvalidateActivityRequest request,
        IActivityManagementService service, ICurrentTenant tenant, ICurrentUser user, HttpContext context, CancellationToken ct)
    {
        if (user.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        return await Manage(activityId, service, tenant, context, (org, id) => service.InvalidateAsync(org, new CrmActivityId(id), user.UserId.Value, request.Reason, ct));
    }
    private static async Task<IResult> RetrySyncAsync(Guid activityId, IActivityManagementService service,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct) =>
        await Manage(activityId, service, tenant, context, (org, id) => service.RetrySyncAsync(org, new CrmActivityId(id), ct));
    private static async Task<IResult> AbandonSyncAsync(Guid activityId, IActivityManagementService service,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct) =>
        await Manage(activityId, service, tenant, context, (org, id) => service.AbandonSyncAsync(org, new CrmActivityId(id), ct));
    private static async Task<IResult> Manage(Guid id, IActivityManagementService _, ICurrentTenant tenant,
        HttpContext context, Func<OrganizationId, Guid, Task<Result>> action)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result result = await action(tenant.OrganizationId.Value, id);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static CrmActivityReadScope ReadScope(ICurrentUser user) =>
        user.HasPermission("Crm.Activities.InternalNotes.Read")
            ? CrmActivityReadScope.IncludeInternal
            : CrmActivityReadScope.PublicOnly;

    private static async Task<IResult> UploadAttachmentAsync(Guid activityId, IFormFile file,
        IActivityAttachmentService service, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        await using Stream stream = file.OpenReadStream();
        Result result = await service.UploadAsync(tenant.OrganizationId.Value,
            new CrmActivityId(activityId), file.FileName, file.ContentType, file.Length, stream, ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> DownloadAttachmentAsync(Guid activityId,
        IActivityAttachmentService service, ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<ActivityAttachmentDownload> result = await service.DownloadAsync(
            tenant.OrganizationId.Value, new CrmActivityId(activityId), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) :
            Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName, enableRangeProcessing: true);
    }

    private static async Task<IResult> DeleteAttachmentAsync(Guid activityId,
        IActivityAttachmentService service, ICurrentTenant tenant, ICurrentUser user,
        HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (user.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await service.DeleteAsync(tenant.OrganizationId.Value,
            new CrmActivityId(activityId), user.UserId.Value, ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }
}

public sealed record CreateCrmActivityRequest(CrmActivityType Type,
    CrmActivityDirection Direction, string Subject, string? Details,
    DateTimeOffset OccurredAtUtc, Guid? AdvisorUserId = null, string? Result = null,
    int? DurationMinutes = null, bool IsInternal = false, bool IsUnfollowed = false,
    bool RequiresRegularization = false, string? AttachmentName = null,
    string? AttachmentReference = null, string? NextActionTitle = null,
    DateTimeOffset? NextActionDueAtUtc = null,
    DriveOS.Modules.CRM.Domain.Tasks.CrmTaskType NextActionType = DriveOS.Modules.CRM.Domain.Tasks.CrmTaskType.FollowUp)
{
    public CrmActivityMetadata ToMetadata() => CrmActivityMetadata.Manual(Result, DurationMinutes,
        IsInternal, IsUnfollowed, RequiresRegularization);
}
public sealed record AttachActivityRequest(Guid LeadId);
public sealed record InvalidateActivityRequest(string Reason);
public sealed record ImportCrmActivityRequest(Guid? LeadId, CrmActivityType Type,
    CrmActivityDirection Direction, string Subject, string? Details, DateTimeOffset OccurredAtUtc,
    Guid? AdvisorUserId, string ExternalId, string IdempotencyKey,
    CrmActivitySyncStatus SyncStatus, string? SyncErrorKey = null, string? Result = null,
    int? DurationMinutes = null, bool RequiresRegularization = false,
    string? AttachmentName = null, string? AttachmentReference = null);
