using DomainRelay.Abstractions;
using DriveOS.Api.Contracts;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Pagination;
using DriveOS.Application.Abstractions.Sorting;
using DriveOS.Modules.CRM.Application.Leads.BulkActions;
using DriveOS.Modules.CRM.Application.Leads.ChangeLeadStatus;
using DriveOS.Modules.CRM.Application.Leads.ConvertLead;
using DriveOS.Modules.CRM.Application.Leads.CreateLead;
using DriveOS.Modules.CRM.Application.Leads.ExportLeads;
using DriveOS.Modules.CRM.Application.Leads.GetLead;
using DriveOS.Modules.CRM.Application.Leads.GetLeads;
using DriveOS.Modules.CRM.Application.Leads.ManageLifecycle;
using DriveOS.Modules.CRM.Application.Leads.QualifyLead;
using DriveOS.Modules.CRM.Application.Leads.SavedViews;
using DriveOS.Modules.CRM.Application.Leads.UpdateLead;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Security.Contracts;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.Crm;

public static class LeadEndpoints
{
    public static IEndpointRouteBuilder MapLeadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/crm/leads").WithTags("CRM - Leads");

        group
            .MapPost("/", CreateLeadAsync)
            .WithName("CreateLead")
            .WithSummary("Créer un prospect")
            .Accepts<CreateLeadRequest>("application/json")
            .Produces<CreateLeadResponse>(StatusCodes.Status201Created)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Create);

        group
            .MapGet("/{leadId:guid}", GetLeadAsync)
            .WithName("GetLead")
            .WithSummary("Obtenir le détail d'un prospect")
            .Produces<LeadResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Read);

        group
            .MapGet("/", GetLeadsAsync)
            .WithName("GetLeads")
            .WithSummary("Lister les prospects")
            .Produces<PagedResponse<LeadListItem>>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Read);

        group
            .MapGet("/export", ExportLeadsAsync)
            .WithName("ExportLeads")
            .WithSummary("Exporter les prospects filtrés au format CSV")
            .Produces(StatusCodes.Status200OK, contentType: "text/csv")
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Export);

        group
            .MapGet("/saved-views", GetSavedViewsAsync)
            .WithName("GetSavedLeadViews")
            .WithSummary("Lister les vues Prospect accessibles")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Read);
        group
            .MapPut("/saved-views", SaveViewAsync)
            .WithName("SaveLeadView")
            .WithSummary("Créer ou modifier une vue Prospect")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.ManageSavedViews);
        group
            .MapDelete("/saved-views/{viewId:guid}", DeleteViewAsync)
            .WithName("DeleteLeadView")
            .WithSummary("Supprimer une vue Prospect personnelle")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.ManageSavedViews);
        group
            .MapPost("/bulk-actions", ExecuteBulkActionAsync)
            .WithName("ExecuteLeadBulkAction")
            .WithSummary("Exécuter une action groupée sur 200 prospects maximum")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.BulkManage);

        group
            .MapPut("/{leadId:guid}", UpdateLeadAsync)
            .WithName("UpdateLead")
            .WithSummary("Modifier les informations d'un prospect")
            .Accepts<UpdateLeadRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Update);

        MapLifecycleEndpoint(
            group,
            "contact",
            LeadStatus.Contacted,
            "Marquer le prospect comme contacté"
        );
        MapLifecycleEndpoint(
            group,
            "schedule-assessment",
            LeadStatus.AssessmentScheduled,
            "Planifier l'évaluation"
        );
        MapLifecycleEndpoint(
            group,
            "send-offer",
            LeadStatus.OfferSent,
            "Marquer l'offre comme envoyée"
        );
        MapLifecycleEndpoint(
            group,
            "start-negotiation",
            LeadStatus.Negotiation,
            "Démarrer la négociation"
        );
        MapLifecycleEndpoint(group, "win", LeadStatus.Won, "Marquer le prospect comme gagné");
        MapLifecycleEndpoint(group, "lose", LeadStatus.Lost, "Marquer le prospect comme perdu");
        MapLifecycleEndpoint(
            group,
            "put-on-hold",
            LeadStatus.Dormant,
            "Mettre le prospect en sommeil"
        );
        MapLifecycleEndpoint(group, "reactivate", LeadStatus.New, "Réactiver le prospect");

        group
            .MapPost("/{leadId:guid}/status/close", CloseLeadAsync)
            .WithName("CloseLeadStructured")
            .WithSummary("Clôturer un prospect avec un motif structuré")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.MarkLost);
        group
            .MapPost("/{leadId:guid}/status/dormant", SetDormantAsync)
            .WithName("SetLeadDormant")
            .WithSummary("Mettre un prospect en sommeil avec rappel")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.SetDormant);
        group
            .MapPost("/{leadId:guid}/status/refer", ReferToPartnerAsync)
            .WithName("ReferLeadToPartner")
            .WithSummary("Orienter un prospect vers un partenaire avec consentement")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.ReferToPartner);
        group
            .MapPost("/{leadId:guid}/status/reopen", ReopenLeadAsync)
            .WithName("ReopenLead")
            .WithSummary("Réouvrir un prospect clôturé")
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Reopen);

        group
            .MapPut("/{leadId:guid}/qualification", QualifyLeadAsync)
            .WithName("QualifyLead")
            .WithSummary("Qualifier le besoin d'un prospect")
            .Accepts<QualifyLeadRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.Qualify);

        group
            .MapPost("/{leadId:guid}/convert", ConvertLeadAsync)
            .WithName("ConvertLeadToStudent")
            .WithSummary("Demander la conversion idempotente d'un prospect en élève")
            .Accepts<ConvertLeadRequest>("application/json")
            .Produces<ConvertLeadResponse>(StatusCodes.Status200OK)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.CrmConversions.ConvertToStudent);

        return endpoints;
    }

    private static async Task<IResult> GetSavedViewsAsync(
        ISavedLeadViewService service,
        ICurrentTenant tenant,
        ICurrentUser user,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!user.IsAuthenticated || user.UserId is null)
            return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        return Results.Ok(
            await service.ListAsync(
                tenant.OrganizationId.Value,
                user.UserId.Value,
                new HashSet<Guid>(),
                ct
            )
        );
    }

    private static async Task<IResult> SaveViewAsync(
        SaveLeadViewInput request,
        ISavedLeadViewService service,
        ICurrentTenant tenant,
        ICurrentUser user,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!user.IsAuthenticated || user.UserId is null)
            return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        bool canShare = user.HasPermission(DriveOsPermissionCodes.CrmLeads.ShareSavedViews);
        SavedLeadViewDto? result = await service.SaveAsync(
            tenant.OrganizationId.Value,
            user.UserId.Value,
            request,
            canShare,
            ct
        );
        return result is null
            ? Results.BadRequest(new { key = "Crm.Leads.SavedViews.Invalid" })
            : Results.Ok(result);
    }

    private static async Task<IResult> DeleteViewAsync(
        Guid viewId,
        ISavedLeadViewService service,
        ICurrentTenant tenant,
        ICurrentUser user,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!user.IsAuthenticated || user.UserId is null)
            return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        return await service.DeleteAsync(tenant.OrganizationId.Value, user.UserId.Value, viewId, ct)
            ? Results.NoContent()
            : Results.NotFound(new { key = "Crm.Leads.SavedViews.NotFound" });
    }

    private static async Task<IResult> ExecuteBulkActionAsync(
        LeadBulkActionInput request,
        ILeadBulkActionService service,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    )
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (request.LeadIds.Count is < 1 or > 200)
            return Results.BadRequest(new { key = "Crm.Leads.Bulk.InvalidCount" });
        return Results.Ok(await service.ExecuteAsync(tenant.OrganizationId.Value, request, ct));
    }

    private static async Task<IResult> CloseLeadAsync(
        Guid leadId,
        CloseLeadRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    ) =>
        await SendLifecycle(
            leadId,
            tenant,
            context,
            id => new CloseLeadCommand(
                tenant.OrganizationId!.Value,
                id,
                request.Decision,
                request.Reason,
                request.Comment
            ),
            mediator,
            ct
        );

    private static async Task<IResult> SetDormantAsync(
        Guid leadId,
        SetDormantRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    ) =>
        await SendLifecycle(
            leadId,
            tenant,
            context,
            id => new SetLeadDormantCommand(
                tenant.OrganizationId!.Value,
                id,
                request.Reason,
                request.ResumeAtUtc,
                new UserId(request.ResponsibleUserId),
                request.CampaignCode,
                request.Comment
            ),
            mediator,
            ct
        );

    private static async Task<IResult> ReferToPartnerAsync(
        Guid leadId,
        ReferToPartnerRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    ) =>
        await SendLifecycle(
            leadId,
            tenant,
            context,
            id => new ReferLeadToPartnerCommand(
                tenant.OrganizationId!.Value,
                id,
                request.PartnerName,
                request.SharedDataDescription,
                request.ConsentCollectedAtUtc,
                request.Comment
            ),
            mediator,
            ct
        );

    private static async Task<IResult> ReopenLeadAsync(
        Guid leadId,
        ReopenLeadRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct
    ) =>
        await SendLifecycle(
            leadId,
            tenant,
            context,
            id => new ReopenLeadCommand(tenant.OrganizationId!.Value, id, request.Comment),
            mediator,
            ct
        );

    private static async Task<IResult> SendLifecycle<TCommand>(
        Guid leadId,
        ICurrentTenant tenant,
        HttpContext context,
        Func<LeadId, TCommand> commandFactory,
        IMediator mediator,
        CancellationToken ct
    )
        where TCommand : DriveOS.Application.Abstractions.Messaging.ICommand
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (leadId == Guid.Empty)
            return LeadErrors.EmptyId.ToHttpResult(context);
        Result result = await mediator.Send(commandFactory(new LeadId(leadId)), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    public sealed record CloseLeadRequest(
        LeadStatus Decision,
        LeadClosureReason Reason,
        string? Comment
    );

    public sealed record SetDormantRequest(
        LeadClosureReason Reason,
        DateTimeOffset ResumeAtUtc,
        Guid ResponsibleUserId,
        string? CampaignCode,
        string? Comment
    );

    public sealed record ReferToPartnerRequest(
        string PartnerName,
        string SharedDataDescription,
        DateTimeOffset ConsentCollectedAtUtc,
        string? Comment
    );

    public sealed record ReopenLeadRequest(string? Comment);

    private static async Task<IResult> ConvertLeadAsync(
        Guid leadId,
        ConvertLeadRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        if (leadId == Guid.Empty)
            return LeadErrors.EmptyId.ToHttpResult(httpContext);

        Result<ConvertLeadResponse> result = await mediator.Send(
            new ConvertLeadCommand(
                currentTenant.OrganizationId.Value,
                new LeadId(leadId),
                new CommercialOfferId(request.AcceptedOfferId),
                new BranchId(request.BranchId),
                new UserId(request.ResponsibleUserId),
                request.TrainingCode,
                request.IdentityVerified,
                request.ConsentsVerified,
                request.DuplicateCheckCompleted,
                request.GuardianSummary,
                request.PayerSummary,
                request.RequiredDocumentCodes ?? []
            ),
            cancellationToken
        );

        return result.IsFailure ? result.Error.ToHttpResult(httpContext) : Results.Ok(result.Value);
    }

    public sealed record ConvertLeadRequest(
        Guid AcceptedOfferId,
        Guid BranchId,
        Guid ResponsibleUserId,
        string TrainingCode,
        bool IdentityVerified,
        bool ConsentsVerified,
        bool DuplicateCheckCompleted,
        string? GuardianSummary,
        string? PayerSummary,
        IReadOnlyCollection<string>? RequiredDocumentCodes
    );

    private static async Task<IResult> QualifyLeadAsync(
        Guid leadId,
        QualifyLeadRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        if (leadId == Guid.Empty)
            return LeadErrors.EmptyId.ToHttpResult(httpContext);

        var command = new QualifyLeadCommand(
            currentTenant.OrganizationId.Value,
            new LeadId(leadId),
            request.Need,
            request.LicenseCategory,
            request.Availability,
            request.TargetDate,
            request.Financing,
            request.Notes
        );
        Result result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(httpContext) : Results.NoContent();
    }

    private static void MapLifecycleEndpoint(
        RouteGroupBuilder group,
        string action,
        LeadStatus targetStatus,
        string summary
    )
    {
        group
            .MapPost(
                $"/{{leadId:guid}}/lifecycle/{action}",
                (
                    Guid leadId,
                    ChangeLeadStatusRequest request,
                    IMediator mediator,
                    ICurrentTenant currentTenant,
                    HttpContext httpContext,
                    CancellationToken cancellationToken
                ) =>
                    ChangeLeadStatusAsync(
                        leadId,
                        targetStatus,
                        request,
                        mediator,
                        currentTenant,
                        httpContext,
                        cancellationToken
                    )
            )
            .WithName($"ChangeLeadStatus{targetStatus}")
            .WithSummary(summary)
            .Accepts<ChangeLeadStatusRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ApiErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiErrorResponse>(StatusCodes.Status401Unauthorized)
            .Produces<ApiErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ApiErrorResponse>(StatusCodes.Status409Conflict)
            .RequireAuthorization(DriveOsPermissionCodes.CrmLeads.ChangeStatus);
    }

    private static async Task<IResult> ChangeLeadStatusAsync(
        Guid leadId,
        LeadStatus targetStatus,
        ChangeLeadStatusRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
        {
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        }

        if (leadId == Guid.Empty)
        {
            return LeadErrors.EmptyId.ToHttpResult(httpContext);
        }

        var command = new ChangeLeadStatusCommand(
            currentTenant.OrganizationId.Value,
            new LeadId(leadId),
            targetStatus,
            request.Reason
        );

        Result result = await mediator.Send(command, cancellationToken);
        return result.IsFailure ? result.Error.ToHttpResult(httpContext) : Results.NoContent();
    }

    private static async Task<IResult> UpdateLeadAsync(
        Guid leadId,
        UpdateLeadRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
        {
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        }

        if (leadId == Guid.Empty)
        {
            return LeadErrors.EmptyId.ToHttpResult(httpContext);
        }

        var command = new UpdateLeadCommand(
            currentTenant.OrganizationId.Value,
            new LeadId(leadId),
            request.BranchId is null ? null : new BranchId(request.BranchId.Value),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.LicenseCategory,
            request.Transmission,
            request.PreferredLocation,
            request.SourceType,
            request.SourceDetail
        );

        Result result = await mediator.Send(command, cancellationToken);

        return result.IsFailure ? result.Error.ToHttpResult(httpContext) : Results.NoContent();
    }

    private static async Task<IResult> GetLeadAsync(
        Guid leadId,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
        {
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        }

        if (leadId == Guid.Empty)
        {
            return LeadErrors.EmptyId.ToHttpResult(httpContext);
        }

        var query = new GetLeadQuery(currentTenant.OrganizationId.Value, new LeadId(leadId));

        Result<LeadResponse> result = await mediator.Send(query, cancellationToken);

        return result.IsFailure ? result.Error.ToHttpResult(httpContext) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetLeadsAsync(
        [AsParameters] GetLeadsRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
        {
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        }

        var query = new GetLeadsQuery(
            currentTenant.OrganizationId.Value,
            request.PageNumber,
            request.PageSize,
            request.Search,
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            ParseOptionalEnum<LeadStatus>(request.Status),
            ParseOptionalEnum<LeadSourceType>(request.SourceType),
            request.AssignedAdvisorId.HasValue ? new UserId(request.AssignedAdvisorId.Value) : null,
            request.UnassignedOnly,
            ParseSortField(request.SortBy),
            ParseSortDirection(request.SortDirection)
        );

        Result<PagedResult<LeadListItem>> result = await mediator.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(httpContext);
        }

        PagedResult<LeadListItem> page = result.Value;
        return Results.Ok(
            new PagedResponse<LeadListItem>(
                page.Items,
                page.PageNumber,
                page.PageSize,
                page.TotalCount,
                page.TotalPages,
                page.HasPreviousPage,
                page.HasNextPage
            )
        );
    }

    private static async Task<IResult> ExportLeadsAsync(
        [AsParameters] GetLeadsRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);

        var query = new ExportLeadsQuery(
            currentTenant.OrganizationId.Value,
            request.Search,
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            ParseOptionalEnum<LeadStatus>(request.Status),
            ParseOptionalEnum<LeadSourceType>(request.SourceType),
            request.AssignedAdvisorId.HasValue ? new UserId(request.AssignedAdvisorId.Value) : null,
            request.UnassignedOnly
        );
        Result<LeadExportFile> result = await mediator.Send(query, cancellationToken);
        return result.IsFailure
            ? result.Error.ToHttpResult(httpContext)
            : Results.File(result.Value.Content, "text/csv; charset=utf-8", result.Value.FileName);
    }

    private static TEnum? ParseOptionalEnum<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse(value.Trim(), true, out TEnum parsed)
            ? parsed
            : (TEnum)Enum.ToObject(typeof(TEnum), -1);
    }

    private static LeadSortField ParseSortField(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "lastname" => LeadSortField.LastName,
            "firstname" => LeadSortField.FirstName,
            "status" => LeadSortField.Status,
            "sourcetype" or "source" => LeadSortField.SourceType,
            "licensecategory" or "training" => LeadSortField.LicenseCategory,
            _ => LeadSortField.CreatedAtUtc,
        };

    private static SortDirection ParseSortDirection(string? value) =>
        string.Equals(value, "asc", StringComparison.OrdinalIgnoreCase)
            ? SortDirection.Ascending
            : SortDirection.Descending;

    private static async Task<IResult> CreateLeadAsync(
        CreateLeadRequest request,
        IMediator mediator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        HttpContext httpContext,
        CancellationToken cancellationToken
    )
    {
        if (!currentTenant.HasTenant || currentTenant.OrganizationId is null)
        {
            return LeadErrors.CurrentTenantRequired.ToHttpResult(httpContext);
        }

        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return LeadErrors.CurrentUserRequired.ToHttpResult(httpContext);
        }

        var command = new CreateLeadCommand(
            currentTenant.OrganizationId.Value,
            request.BranchId is null ? null : new BranchId(request.BranchId.Value),
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            request.LicenseCategory,
            request.Transmission,
            request.PreferredLocation,
            request.SourceType,
            request.SourceDetail,
            request.AssignedAdvisorId is null ? null : new UserId(request.AssignedAdvisorId.Value)
        );

        Result<LeadId> result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToHttpResult(httpContext);
        }

        Guid leadId = result.Value.Value;
        return Results.Created($"/api/crm/leads/{leadId:D}", new CreateLeadResponse(leadId));
    }
}

public sealed record CreateLeadRequest(
    Guid? BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string LicenseCategory,
    TransmissionPreference Transmission,
    string? PreferredLocation,
    LeadSourceType SourceType,
    string? SourceDetail,
    Guid? AssignedAdvisorId
);

public sealed record CreateLeadResponse(Guid LeadId);

public sealed record UpdateLeadRequest(
    Guid? BranchId,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    string LicenseCategory,
    TransmissionPreference Transmission,
    string? PreferredLocation,
    LeadSourceType SourceType,
    string? SourceDetail
);

public sealed record ChangeLeadStatusRequest(string? Reason);

public sealed record QualifyLeadRequest(
    string Need,
    string LicenseCategory,
    string Availability,
    DateOnly? TargetDate,
    FinancingOption Financing,
    string? Notes
);

public sealed record GetLeadsRequest(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null,
    Guid? BranchId = null,
    string? Status = null,
    string? SourceType = null,
    Guid? AssignedAdvisorId = null,
    bool UnassignedOnly = false,
    string? SortBy = null,
    string? SortDirection = null
);
