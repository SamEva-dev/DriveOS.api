using DomainRelay.Abstractions;
using DriveOS.Api.Errors;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.CRM.Application.Assessments.CancelAssessment;
using DriveOS.Modules.CRM.Application.Assessments.GetAssessments;
using DriveOS.Modules.CRM.Application.Assessments.RescheduleAssessment;
using DriveOS.Modules.CRM.Application.Assessments.ScheduleAssessment;
using DriveOS.Modules.CRM.Application.Assessments.PerformAssessment;
using DriveOS.Modules.CRM.Application.Assessments.AssessmentResult;
using DriveOS.Modules.CRM.Domain.Leads;
using DriveOS.Modules.CRM.Domain.Assessments;
using DriveOS.Modules.Organizations.Domain.Branches;
using DriveOS.Modules.Organizations.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Api.Endpoints.Crm;

public static class AssessmentAppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAssessmentAppointmentEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/crm")
            .WithTags("CRM - Assessments");

        group.MapGet("/leads/{leadId:guid}/assessments", GetByLeadAsync)
            .RequireAuthorization("Crm.Assessments.Read");
        group.MapGet("/assessments/{appointmentId:guid}", GetByIdAsync)
            .RequireAuthorization("Crm.Assessments.Read");
        group.MapPost("/leads/{leadId:guid}/assessments", ScheduleAsync)
            .RequireAuthorization("Crm.Assessments.Schedule", "Branches.Read");
        group.MapPut("/assessments/{appointmentId:guid}/schedule", RescheduleAsync)
            .RequireAuthorization("Crm.Assessments.Schedule");
        group.MapPost("/assessments/{appointmentId:guid}/cancel", CancelAsync)
            .RequireAuthorization("Crm.Assessments.Cancel");
        group.MapPost("/assessments/{appointmentId:guid}/session/start", StartSessionAsync)
            .RequireAuthorization("Crm.Assessments.Start");
        group.MapGet("/assessments/{appointmentId:guid}/session", GetSessionAsync)
            .RequireAuthorization("Crm.Assessments.Read");
        group.MapPut("/assessments/{appointmentId:guid}/session/draft", SaveDraftAsync)
            .RequireAuthorization("Crm.Assessments.Complete", "Crm.AssessmentNotes.Create");
        group.MapPost("/assessments/{appointmentId:guid}/session/submit", SubmitSessionAsync)
            .RequireAuthorization("Crm.Assessments.Submit");
        group.MapPut("/assessments/{appointmentId:guid}/result", SaveResultAsync)
            .RequireAuthorization("Crm.Assessments.Result.Create");
        group.MapGet("/assessments/{appointmentId:guid}/result", GetResultAsync)
            .RequireAuthorization("Crm.Assessments.Result.Read");
        group.MapPost("/assessments/{appointmentId:guid}/result/request-correction", RequestResultCorrectionAsync)
            .RequireAuthorization("Crm.Assessments.Result.Validate");
        group.MapPost("/assessments/{appointmentId:guid}/result/validate", ValidateResultAsync)
            .RequireAuthorization("Crm.Assessments.Result.Validate");
        group.MapPost("/assessments/{appointmentId:guid}/result/share", ShareResultAsync)
            .RequireAuthorization("Crm.Assessments.Result.Share");

        return endpoints;
    }

    private static async Task<IResult> GetByLeadAsync(
        Guid leadId,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<IReadOnlyList<AssessmentAppointmentResponse>> result = await mediator.Send(
            new GetLeadAssessmentsQuery(tenant.OrganizationId.Value, new LeadId(leadId)),
            ct);

        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid appointmentId,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result<AssessmentAppointmentResponse> result = await mediator.Send(
            new GetAssessmentQuery(tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId)),
            ct);

        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> ScheduleAsync(
        Guid leadId,
        ScheduleAssessmentRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        OrganizationsDbContext organizationsDbContext,
        HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        BranchId? branchId = request.BranchId is null
            ? null
            : new BranchId(request.BranchId.Value);

        if (branchId.HasValue)
        {
            bool branchAvailable = await organizationsDbContext.Branches
                .AsNoTracking()
                .AnyAsync(branch =>
                    branch.OrganizationId == tenant.OrganizationId.Value &&
                    branch.Id == branchId.Value &&
                    branch.Status != BranchStatus.Closed,
                    ct);

            if (!branchAvailable)
                return AssessmentAppointmentErrors.BranchNotAvailable.ToHttpResult(context);
        }

        Result<Guid> result = await mediator.Send(
            new ScheduleAssessmentCommand(
                tenant.OrganizationId.Value,
                new LeadId(leadId),
                branchId,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.Type,
                request.DeliveryMode,
                request.LocationKind,
                request.LocationDetails,
                request.EvaluatorUserId is null ? null : new UserId(request.EvaluatorUserId.Value),
                request.VehicleId,
                request.RoomId,
                request.SimulatorId,
                request.PriceAmount,
                request.PriceCurrency,
                request.Notes),
            ct);

        return result.IsFailure
            ? result.Error.ToHttpResult(context)
            : Results.Created($"/api/crm/assessments/{result.Value}", new { appointmentId = result.Value });
    }

    private static async Task<IResult> RescheduleAsync(
        Guid appointmentId,
        RescheduleAssessmentRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result result = await mediator.Send(
            new RescheduleAssessmentCommand(
                tenant.OrganizationId.Value,
                new AssessmentAppointmentId(appointmentId),
                request.StartsAtUtc,
                request.EndsAtUtc),
            ct);

        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> CancelAsync(
        Guid appointmentId,
        IMediator mediator,
        ICurrentTenant tenant,
        HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null)
            return LeadErrors.CurrentTenantRequired.ToHttpResult(context);

        Result result = await mediator.Send(
            new CancelAssessmentCommand(
                tenant.OrganizationId.Value,
                new AssessmentAppointmentId(appointmentId),
                DateTimeOffset.UtcNow),
            ct);

        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> StartSessionAsync(Guid appointmentId, StartAssessmentSessionRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result<Guid> result = await mediator.Send(new StartAssessmentCommand(tenant.OrganizationId.Value,
            new AssessmentAppointmentId(appointmentId), currentUser.UserId.Value, request.QuestionnaireCode, request.QuestionnaireVersion,
            request.QuestionnaireSnapshot.GetRawText(), DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Created($"/api/crm/assessments/{appointmentId}/session", new { sessionId = result.Value });
    }

    private static async Task<IResult> GetSessionAsync(Guid appointmentId, IMediator mediator,
        ICurrentTenant tenant, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<AssessmentSessionResponse> result = await mediator.Send(new GetAssessmentSessionQuery(tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId)), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.Ok(result.Value);
    }

    private static async Task<IResult> SaveDraftAsync(Guid appointmentId, SaveAssessmentDraftRequest request,
        IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await mediator.Send(new SaveAssessmentDraftCommand(tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId), currentUser.UserId.Value,
            request.Answers.GetRawText(), request.FactualObservations, request.PedagogicalInterpretation,
            request.Recommendation, request.InternalNotes, request.ProspectComment, request.DraftCompleted,
            DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> SubmitSessionAsync(Guid appointmentId, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await mediator.Send(new SubmitAssessmentCommand(tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId),
            currentUser.UserId.Value, DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> SaveResultAsync(Guid appointmentId,
        SaveAssessmentResultRequest request, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await mediator.Send(new SaveAssessmentResultCommand(
            tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId), currentUser.UserId.Value,
            request.ExpectedRevision, request.Result.GetRawText(), request.Confidence,
            request.AiSuggestion?.GetRawText(), DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> GetResultAsync(Guid appointmentId,
        IMediator mediator, ICurrentTenant tenant, HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        Result<AssessmentSessionResponse> result = await mediator.Send(
            new GetAssessmentSessionQuery(tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId)), ct);
        if (result.IsFailure) return result.Error.ToHttpResult(context);
        AssessmentSessionResponse session = result.Value;
        return Results.Ok(new AssessmentResultResponse(
            session.SessionId, session.AppointmentId, session.LeadId, session.Revision,
            session.ResultJson, session.AiSuggestionJson, session.ResultConfidence,
            session.ResultStatus, session.CorrectionReason, session.InternalNotes,
            session.ResultValidatedAtUtc, session.ResultValidatedByUserId,
            session.ResultSharedAtUtc, session.ResultSharedByUserId));
    }

    private static async Task<IResult> RequestResultCorrectionAsync(Guid appointmentId,
        RequestAssessmentResultCorrectionRequest request, IMediator mediator,
        ICurrentTenant tenant, ICurrentUser currentUser, HttpContext context,
        CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await mediator.Send(new RequestAssessmentResultCorrectionCommand(
            tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId), currentUser.UserId.Value,
            request.ExpectedRevision, request.Reason, DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> ValidateResultAsync(Guid appointmentId,
        AssessmentResultTransitionRequest request, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await mediator.Send(new ValidateAssessmentResultCommand(
            tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId), currentUser.UserId.Value,
            request.ExpectedRevision, DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }

    private static async Task<IResult> ShareResultAsync(Guid appointmentId,
        AssessmentResultTransitionRequest request, IMediator mediator, ICurrentTenant tenant,
        ICurrentUser currentUser, HttpContext context, CancellationToken ct)
    {
        if (!tenant.HasTenant || tenant.OrganizationId is null) return LeadErrors.CurrentTenantRequired.ToHttpResult(context);
        if (!currentUser.IsAuthenticated || currentUser.UserId is null) return LeadErrors.CurrentUserRequired.ToHttpResult(context);
        Result result = await mediator.Send(new ShareAssessmentResultCommand(
            tenant.OrganizationId.Value, new AssessmentAppointmentId(appointmentId), currentUser.UserId.Value,
            request.ExpectedRevision, DateTimeOffset.UtcNow), ct);
        return result.IsFailure ? result.Error.ToHttpResult(context) : Results.NoContent();
    }
}

public sealed record StartAssessmentSessionRequest(string QuestionnaireCode, int QuestionnaireVersion, System.Text.Json.JsonElement QuestionnaireSnapshot);
public sealed record SaveAssessmentDraftRequest(System.Text.Json.JsonElement Answers, string? FactualObservations,
    string? PedagogicalInterpretation, string? Recommendation, string? InternalNotes,
    string? ProspectComment, bool DraftCompleted);
public sealed record SaveAssessmentResultRequest(int ExpectedRevision,
    System.Text.Json.JsonElement Result, AssessmentResultConfidence Confidence,
    System.Text.Json.JsonElement? AiSuggestion);
public sealed record RequestAssessmentResultCorrectionRequest(int ExpectedRevision, string Reason);
public sealed record AssessmentResultTransitionRequest(int ExpectedRevision);
public sealed record AssessmentResultResponse(Guid SessionId, Guid AppointmentId, Guid LeadId,
    int Revision, string? ResultJson, string? AiSuggestionJson, string? Confidence,
    string Status, string? CorrectionReason, string? InternalNotes,
    DateTimeOffset? ValidatedAtUtc, Guid? ValidatedByUserId,
    DateTimeOffset? SharedAtUtc, Guid? SharedByUserId);

public sealed record ScheduleAssessmentRequest(
    Guid? BranchId,
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc,
    AssessmentType Type,
    AssessmentDeliveryMode DeliveryMode,
    AssessmentLocationKind LocationKind,
    string? LocationDetails,
    Guid? EvaluatorUserId,
    Guid? VehicleId,
    Guid? RoomId,
    Guid? SimulatorId,
    decimal? PriceAmount,
    string? PriceCurrency,
    string? Notes);

public sealed record RescheduleAssessmentRequest(
    DateTimeOffset StartsAtUtc,
    DateTimeOffset EndsAtUtc);
