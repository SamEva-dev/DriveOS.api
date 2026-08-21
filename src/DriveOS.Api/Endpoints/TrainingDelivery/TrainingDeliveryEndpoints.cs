using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Api.Integrations.TrainingDelivery;
using DriveOS.Modules.TrainingDelivery.Application.Incidents;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Application.Cancellations;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.TrainingDelivery;

internal static class TrainingDeliveryEndpoints
{
    internal static IEndpointRouteBuilder MapTrainingDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/training-delivery").WithTags("Training Delivery");
        group.MapGet("/dashboard", GetDashboard).RequireAuthorization("TrainingDelivery.Sessions.Read");
        group.MapGet("/my-day", GetMyDay).RequireAuthorization("TrainingDelivery.Sessions.Read");
        group.MapGet("/pending-reports", GetPendingReports).RequireAuthorization("TrainingDelivery.Reports.Read");
        group.MapPost("/sessions/from-booking/{bookingId:guid}", Materialize).RequireAuthorization("TrainingDelivery.Sessions.Materialize");
        group.MapGet("/sessions/{sessionId:guid}", Get).RequireAuthorization("TrainingDelivery.Sessions.Read");
        group.MapPost("/sessions/{sessionId:guid}/prepare", Prepare).RequireAuthorization("TrainingDelivery.Sessions.Prepare");
        group.MapPost("/sessions/{sessionId:guid}/start", Start).RequireAuthorization("TrainingDelivery.Sessions.Start");
        group.MapPost("/sessions/{sessionId:guid}/attendance", RecordAttendance).RequireAuthorization("TrainingDelivery.Attendance.Record");
        group.MapPost("/sessions/{sessionId:guid}/attendance/correct", CorrectAttendance).RequireAuthorization("TrainingDelivery.Attendance.Correct");
        group.MapPost("/sessions/{sessionId:guid}/attendance/override", OverrideAttendance).RequireAuthorization("TrainingDelivery.Attendance.Override");
        group.MapPost("/sessions/{sessionId:guid}/interventions", RecordIntervention).RequireAuthorization("TrainingDelivery.Execution.Interventions.Record");
        group.MapPost("/sessions/{sessionId:guid}/observations", RecordObservation).RequireAuthorization("TrainingDelivery.Execution.Observations.Record");
        group.MapPost("/sessions/{sessionId:guid}/markers", RecordMarker).RequireAuthorization("TrainingDelivery.Execution.Observations.Record");
        group.MapPost("/sessions/{sessionId:guid}/interrupt", Interrupt).RequireAuthorization("TrainingDelivery.Execution.Interrupt");
        group.MapPost("/sessions/{sessionId:guid}/resume", Resume).RequireAuthorization("TrainingDelivery.Execution.Resume");
        group.MapPost("/sessions/{sessionId:guid}/odometer", RecordOdometer).RequireAuthorization("TrainingDelivery.Execution.Odometer.Record");
        group.MapPost("/sessions/{sessionId:guid}/energy", RecordEnergy).RequireAuthorization("TrainingDelivery.Execution.Odometer.Record");
        group.MapPost("/sessions/{sessionId:guid}/assessments", RecordCompetencyAssessment).RequireAuthorization("TrainingDelivery.Assessments.Record");
        group.MapPost("/sessions/{sessionId:guid}/finish", Finish).RequireAuthorization("TrainingDelivery.Sessions.Complete");
        group.MapPut("/sessions/{sessionId:guid}/report/draft", SaveReportDraft).RequireAuthorization("TrainingDelivery.Reports.Write");
        group.MapPut("/sessions/{sessionId:guid}/report/shared-comment", UpdateSharedComment).RequireAuthorization("TrainingDelivery.SessionComments.CreateShared");
        group.MapPut("/sessions/{sessionId:guid}/report/internal-note", UpdateInternalNote).RequireAuthorization("TrainingDelivery.SessionNotes.CreateInternal");
        group.MapGet("/sessions/{sessionId:guid}/report/internal-note", GetInternalNote).RequireAuthorization("TrainingDelivery.SessionNotes.ReadInternal");
        group.MapGet("/sessions/{sessionId:guid}/report/review", GetReportReview).RequireAuthorization("TrainingDelivery.Reports.Read");
        group.MapPost("/sessions/{sessionId:guid}/report/ready", MarkReportReady).RequireAuthorization("TrainingDelivery.Reports.Submit");
        group.MapPost("/sessions/{sessionId:guid}/report/submit", SubmitReport).RequireAuthorization("TrainingDelivery.Reports.Submit");
        group.MapGet("/sessions/{sessionId:guid}/report/revisions", GetReportRevisions).RequireAuthorization("TrainingDelivery.Reports.RequestCorrection");
        group.MapPost("/sessions/{sessionId:guid}/report/revisions", RequestReportRevision).RequireAuthorization("TrainingDelivery.Reports.RequestCorrection");
        group.MapPost("/sessions/{sessionId:guid}/report/revisions/{revisionId:guid}/decision", DecideReportRevision).RequireAuthorization("TrainingDelivery.Reports.ApproveCorrection");
        group.MapPost("/sessions/{sessionId:guid}/cancel-execution", CancelExecution).RequireAuthorization("TrainingDelivery.Cancellations.Record");
        group.MapGet("/sessions/{sessionId:guid}/cancellation", GetCancellation).RequireAuthorization("TrainingDelivery.Cancellations.Read");
        group.MapGet("/sessions/{sessionId:guid}/cancellation/consequences", GetCancellationConsequences).RequireAuthorization("TrainingDelivery.Consequences.Read");
        group.MapPost("/sessions/{sessionId:guid}/cancellation/consequences/retry", RetryCancellationConsequences).RequireAuthorization("TrainingDelivery.Consequences.Retry");
        group.MapPost("/sessions/{sessionId:guid}/incidents", ReportIncident).RequireAuthorization("TrainingDelivery.Incidents.Report");
        group.MapGet("/sessions/{sessionId:guid}/incidents", GetSessionIncidents).RequireAuthorization("TrainingDelivery.Incidents.Read");
        group.MapGet("/incidents/{incidentId:guid}", GetIncident).RequireAuthorization("TrainingDelivery.Incidents.Read");
        group.MapPost("/incidents/{incidentId:guid}/evidence", AddIncidentEvidence).RequireAuthorization("TrainingDelivery.Incidents.Update");
        group.MapPost("/incidents/{incidentId:guid}/escalate", EscalateIncident).RequireAuthorization("TrainingDelivery.Incidents.Escalate");
        group.MapPost("/incidents/{incidentId:guid}/review", StartIncidentReview).RequireAuthorization("TrainingDelivery.Incidents.Update");
        group.MapPost("/incidents/{incidentId:guid}/resolve", ResolveIncident).RequireAuthorization("TrainingDelivery.Incidents.Resolve");
        group.MapPost("/incidents/{incidentId:guid}/close", CloseIncident).RequireAuthorization("TrainingDelivery.Incidents.Close");
        group.MapGet("/sessions/{sessionId:guid}/consequences", GetConsequences).RequireAuthorization("TrainingDelivery.Consequences.Read");
        group.MapPost("/sessions/{sessionId:guid}/consequences/retry", RetryConsequences).RequireAuthorization("TrainingDelivery.Consequences.Retry");
        return app;
    }

    private static async Task<IResult> GetDashboard(
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        ITrainingDeliveryDashboardReadService service,
        ICurrentTenant tenant,
        IClock clock,
        CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        if (endAtUtc <= startAtUtc)
            return Results.BadRequest(new
            {
                code = "TrainingDelivery.Dashboard.Period.Invalid",
                messageKey = "errors.trainingDelivery.dashboard.period.invalid"
            });

        TrainingDeliveryDashboardResponse response = await service.GetAsync(
            organizationId,
            startAtUtc,
            endAtUtc,
            clock.UtcNow,
            null,
            ct);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetMyDay(
        DateTimeOffset startAtUtc,
        DateTimeOffset endAtUtc,
        ITrainingDeliveryDashboardReadService service,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        IClock clock,
        CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } instructorId)
            return Results.Unauthorized();

        if (endAtUtc <= startAtUtc)
            return Results.BadRequest(new
            {
                code = "TrainingDelivery.Dashboard.Period.Invalid",
                messageKey = "errors.trainingDelivery.dashboard.period.invalid"
            });

        TrainingDeliveryDashboardResponse response = await service.GetAsync(
            organizationId,
            startAtUtc,
            endAtUtc,
            clock.UtcNow,
            instructorId,
            ct);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetPendingReports(
        bool? mineOnly,
        ITrainingDeliveryPendingReportsReadService service,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        IClock clock,
        CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } currentUserId)
            return Results.Unauthorized();

        TrainingDeliveryPendingReportsResponse response = await service.GetAsync(
            organizationId,
            currentUserId,
            currentUser.HasPermission("TrainingDelivery.Reports.Monitor"),
            mineOnly ?? false,
            clock.UtcNow,
            ct);

        return Results.Ok(response);
    }

    private static async Task<IResult> Materialize(Guid bookingId, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<TrainingSessionId> result = await mediator.Send(new MaterializeTrainingSessionCommand(organizationId, new BookingId(bookingId), currentUser.UserId), ct);
        return result.IsSuccess ? Results.Ok(new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> Get(Guid sessionId, ITrainingSessionReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        TrainingSessionResponse? session = await service.GetAsync(organizationId, new TrainingSessionId(sessionId), ct);
        return session is null ? Results.NotFound() : Results.Ok(session);
    }

    private static async Task<IResult> Prepare(Guid sessionId, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<TrainingSessionPreparationResponse> result = await mediator.Send(
            new PrepareTrainingSessionCommand(organizationId, new TrainingSessionId(sessionId), actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Start(Guid sessionId, StartSessionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(
            new StartTrainingSessionCommand(organizationId, new TrainingSessionId(sessionId), request.OperationId, request.StartedAtUtc, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }


    private static async Task<IResult> RecordAttendance(Guid sessionId, RecordAttendanceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionAttendanceStatus), request.Status))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Attendance.Status.Invalid", messageKey = "errors.trainingDelivery.session.attendance.status.invalid" });

        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionAttendanceCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionAttendanceStatus)request.Status,
            request.ActualArrivalAtUtc, request.ActualDepartureAtUtc, request.Reason, request.EvidenceDocumentId, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> CorrectAttendance(Guid sessionId, CorrectAttendanceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct) =>
        await CorrectAttendanceCore(sessionId, request, false, mediator, tenant, currentUser, ct);

    private static async Task<IResult> OverrideAttendance(Guid sessionId, CorrectAttendanceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct) =>
        await CorrectAttendanceCore(sessionId, request, true, mediator, tenant, currentUser, ct);

    private static async Task<IResult> CorrectAttendanceCore(Guid sessionId, CorrectAttendanceRequest request, bool isOverride, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionAttendanceStatus), request.Status))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Attendance.Status.Invalid", messageKey = "errors.trainingDelivery.session.attendance.status.invalid" });

        Result<TrainingSessionResponse> result = await mediator.Send(new CorrectTrainingSessionAttendanceCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionAttendanceStatus)request.Status,
            request.ActualArrivalAtUtc, request.ActualDepartureAtUtc, request.Reason, request.EvidenceDocumentId, actorUserId, isOverride, request.OverrideReason), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RecordIntervention(Guid sessionId, RecordInterventionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionInterventionType), request.Type) ||
            !Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionInterventionSeverity), request.Severity))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Intervention.Invalid", messageKey = "errors.trainingDelivery.session.intervention.invalid" });
        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionInterventionCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionInterventionType)request.Type,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionInterventionSeverity)request.Severity,
            request.OccurredAtUtc, request.Context, request.Reason, request.RelatedCompetencyId.HasValue ? new CompetencyId(request.RelatedCompetencyId.Value) : null, request.Outcome, request.InternalComment, request.SharedExplanation, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RecordObservation(Guid sessionId, RecordObservationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionObservationType), request.Type))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Observation.Invalid", messageKey = "errors.trainingDelivery.session.observation.invalid" });
        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionObservationCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionObservationType)request.Type,
            request.ObservedAtUtc, request.Content, request.IsInternal, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RecordMarker(Guid sessionId, RecordMarkerRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionMarkerType), request.Type) ||
            !Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionMarkerSeverity), request.Severity))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Marker.Invalid", messageKey = "errors.trainingDelivery.session.marker.invalid" });

        CompetencyId? competencyId = request.CompetencyId.HasValue ? new CompetencyId(request.CompetencyId.Value) : null;
        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionMarkerCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionMarkerType)request.Type, request.OccurredAtUtc, competencyId, request.ShortNote,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionMarkerSeverity)request.Severity, request.Latitude, request.Longitude, request.CreatedOffline, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Interrupt(Guid sessionId, InterruptSessionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionInterruptionReason), request.Reason))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Interruption.Invalid", messageKey = "errors.trainingDelivery.session.interruption.invalid" });
        Result<TrainingSessionResponse> result = await mediator.Send(new InterruptTrainingSessionCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionInterruptionReason)request.Reason,
            request.Description, request.InterruptedAtUtc, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Resume(Guid sessionId, ResumeSessionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new ResumeTrainingSessionCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ResumedAtUtc, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RecordOdometer(Guid sessionId, RecordOdometerRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionOdometerSource), request.Source))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Odometer.Invalid", messageKey = "errors.trainingDelivery.session.odometer.invalid" });
        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionOdometerCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.OdometerKilometers,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionOdometerSource)request.Source,
            request.ObservedAtUtc, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }



    private static async Task<IResult> RecordEnergy(Guid sessionId, RecordEnergyRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionEnergyEntryType), request.Type))
            return Results.BadRequest(new { code = "TrainingDelivery.Session.Energy.Invalid", messageKey = "errors.trainingDelivery.session.energy.invalid" });
        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionEnergyCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId,
            (DriveOS.Modules.TrainingDelivery.Domain.Sessions.TrainingSessionEnergyEntryType)request.Type, request.EnergyLevelPercent, request.Quantity,
            request.ObservedAtUtc, request.Note, request.CreatedOffline, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RecordCompetencyAssessment(Guid sessionId, RecordCompetencyAssessmentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new RecordTrainingSessionCompetencyAssessmentCommand(
            organizationId,
            new TrainingSessionId(sessionId),
            request.OperationId,
            new CompetencyId(request.CompetencyId),
            request.LevelCode,
            request.ObservedCriteria,
            request.Context,
            request.RelatedInterventionId.HasValue ? new TrainingSessionInterventionId(request.RelatedInterventionId.Value) : null,
            request.InternalComment,
            request.SharedComment,
            request.EvidenceDocumentId,
            request.AssessedAtUtc,
            actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Finish(Guid sessionId, FinishTrainingSessionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new FinishTrainingSessionCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ActualEndAtUtc, request.EndEnergyLevelPercent, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> UpdateSharedComment(Guid sessionId, UpdateTrainingSessionNarrativeRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } userId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new UpdateTrainingSessionSharedCommentCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ExpectedVersion, request.Content, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> UpdateInternalNote(Guid sessionId, UpdateTrainingSessionNarrativeRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } userId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new UpdateTrainingSessionInternalNoteCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ExpectedVersion, request.Content, userId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> GetInternalNote(Guid sessionId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<TrainingSessionInternalNoteResponse> result = await mediator.Send(new GetTrainingSessionInternalNoteQuery(organizationId, new TrainingSessionId(sessionId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> SaveReportDraft(Guid sessionId, SaveTrainingSessionReportDraftRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, Microsoft.AspNetCore.Authorization.IAuthorizationService authorization, HttpContext httpContext, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (request.SharedComment is not null && !(await authorization.AuthorizeAsync(httpContext.User, null, "TrainingDelivery.SessionComments.CreateShared")).Succeeded) return Results.Forbid();
        if (request.InternalNote is not null && !(await authorization.AuthorizeAsync(httpContext.User, null, "TrainingDelivery.SessionNotes.CreateInternal")).Succeeded) return Results.Forbid();
        Result<TrainingSessionResponse> result = await mediator.Send(new SaveTrainingSessionReportDraftCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ExpectedVersion, request.LastCompletedStep,
            request.Summary, request.ObjectivesWorked, request.ObjectivesAchieved, request.NextObjective, request.SharedComment, request.InternalNote, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> GetReportReview(Guid sessionId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<TrainingSessionReportReviewResponse> result = await mediator.Send(new GetTrainingSessionReportReviewQuery(organizationId, new TrainingSessionId(sessionId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> MarkReportReady(Guid sessionId, ReportVersionedOperationRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new MarkTrainingSessionReportReadyCommand(organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ExpectedVersion, actor), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> SubmitReport(Guid sessionId, SubmitTrainingSessionReportRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, Microsoft.AspNetCore.Authorization.IAuthorizationService authorization, HttpContext httpContext, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        if (request.RequestSupervisorReview && !(await authorization.AuthorizeAsync(httpContext.User, null, "TrainingDelivery.Reports.RequestReview")).Succeeded) return Results.Forbid();
        Result<TrainingSessionResponse> result = await mediator.Send(new SubmitTrainingSessionReportCommand(organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ExpectedVersion, request.RequestSupervisorReview, actor), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> GetReportRevisions(Guid sessionId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        Result<IReadOnlyCollection<TrainingSessionReportRevisionResponse>> result = await mediator.Send(new GetTrainingSessionReportRevisionsQuery(organizationId, new TrainingSessionId(sessionId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RequestReportRevision(Guid sessionId, RequestTrainingSessionReportRevisionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingSessionReportRevisionResponse> result = await mediator.Send(new RequestTrainingSessionReportRevisionCommand(organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ExpectedVersion, request.Scenario, request.FieldCode, request.CurrentValue, request.ProposedValue, request.Reason, request.HasFinancialImpact, request.ApprovalRequired, actor), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> DecideReportRevision(Guid sessionId, Guid revisionId, DecideTrainingSessionReportRevisionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingSessionReportRevisionResponse> result = await mediator.Send(new DecideTrainingSessionReportRevisionCommand(organizationId, new TrainingSessionId(sessionId), new TrainingSessionReportRevisionId(revisionId), request.Approve, request.DecisionReason, actor), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> Complete(Guid sessionId, CompleteTrainingSessionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        Result<TrainingSessionResponse> result = await mediator.Send(new CompleteTrainingSessionCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.ActualEndAtUtc,
            request.Summary, request.ObjectivesWorked, request.ObjectivesAchieved, request.NextObjective,
            request.InstructorComments, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }



    private static async Task<IResult> CancelExecution(Guid sessionId, CancelTrainingSessionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(SessionCancellationReason), request.Reason) || !Enum.IsDefined(typeof(SessionCancellationBillingDecision), request.BillingDecision) ||
            !Enum.IsDefined(typeof(SessionCancellationCreditDecision), request.CreditDecision) || !Enum.IsDefined(typeof(SessionCancellationProviderCompensationDecision), request.ProviderCompensationDecision))
            return Results.BadRequest(new { code = "TrainingDelivery.Cancellation.Invalid", messageKey = "errors.trainingDelivery.cancellation.invalid" });
        Result<SessionCancellationResponse> result = await mediator.Send(new CancelTrainingSessionCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, request.CancelledAtUtc, (SessionCancellationReason)request.Reason, request.ReasonDetails,
            (SessionCancellationBillingDecision)request.BillingDecision, (SessionCancellationCreditDecision)request.CreditDecision, request.PartialCreditQuantity,
            (SessionCancellationProviderCompensationDecision)request.ProviderCompensationDecision, request.DecisionReason, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> GetCancellation(Guid sessionId, ITrainingSessionCancellationReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        SessionCancellationResponse? result = await service.GetBySessionAsync(organizationId, new TrainingSessionId(sessionId), ct);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetCancellationConsequences(Guid sessionId, ITrainingSessionCancellationConsequenceStore store, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        return Results.Ok(await store.GetBySessionAsync(organizationId, new TrainingSessionId(sessionId), ct));
    }

    private static async Task<IResult> RetryCancellationConsequences(
        Guid sessionId,
        ITrainingSessionCancellationConsequenceStore store,
        ICurrentTenant tenant,
        IClock clock,
        CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        await store.RequeueAsync(organizationId, new TrainingSessionId(sessionId), clock.UtcNow, ct);
        return Results.Accepted($"/api/training-delivery/sessions/{sessionId}/cancellation/consequences");
    }

    private static async Task<IResult> ReportIncident(Guid sessionId, ReportIncidentRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actorUserId) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(TrainingIncidentType), request.IncidentType) || !Enum.IsDefined(typeof(TrainingIncidentSeverity), request.Severity))
            return Results.BadRequest(new { code = "TrainingDelivery.Incident.Invalid", messageKey = "errors.trainingDelivery.incident.invalid" });
        var participants = request.AdditionalParticipants?.Select(x => new TrainingIncidentParticipantInput(x.Type, x.ReferenceId, x.Label)).ToArray() ?? [];
        Result<TrainingIncidentResponse> result = await mediator.Send(new ReportTrainingIncidentCommand(
            organizationId, new TrainingSessionId(sessionId), request.OperationId, (TrainingIncidentType)request.IncidentType,
            (TrainingIncidentSeverity)request.Severity, request.OccurredAtUtc, request.Description, request.ImmediateActions,
            participants, actorUserId), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> GetSessionIncidents(Guid sessionId, ITrainingIncidentReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        return Results.Ok(await service.GetBySessionAsync(organizationId, new TrainingSessionId(sessionId), ct));
    }

    private static async Task<IResult> GetIncident(Guid incidentId, ITrainingIncidentReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        TrainingIncidentResponse? incident = await service.GetAsync(organizationId, new TrainingIncidentId(incidentId), ct);
        return incident is null ? Results.NotFound() : Results.Ok(incident);
    }

    private static async Task<IResult> AddIncidentEvidence(Guid incidentId, AddIncidentEvidenceRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingIncidentResponse> result = await mediator.Send(new AddTrainingIncidentEvidenceCommand(organizationId,new TrainingIncidentId(incidentId),request.OperationId,request.DocumentId,request.EvidenceType,request.Description,actor),ct);
        return result.IsSuccess?Results.Ok(result.Value):Failure(result.Error);
    }
    private static async Task<IResult> EscalateIncident(Guid incidentId, IncidentReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingIncidentResponse> result=await mediator.Send(new EscalateTrainingIncidentCommand(organizationId,new TrainingIncidentId(incidentId),request.OperationId,request.Reason??string.Empty,actor),ct); return result.IsSuccess?Results.Ok(result.Value):Failure(result.Error);
    }
    private static async Task<IResult> StartIncidentReview(Guid incidentId, IncidentReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingIncidentResponse> result=await mediator.Send(new StartTrainingIncidentReviewCommand(organizationId,new TrainingIncidentId(incidentId),request.OperationId,request.Reason,actor),ct); return result.IsSuccess?Results.Ok(result.Value):Failure(result.Error);
    }
    private static async Task<IResult> ResolveIncident(Guid incidentId, IncidentResolutionRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingIncidentResponse> result=await mediator.Send(new ResolveTrainingIncidentCommand(organizationId,new TrainingIncidentId(incidentId),request.OperationId,request.Resolution,actor),ct); return result.IsSuccess?Results.Ok(result.Value):Failure(result.Error);
    }
    private static async Task<IResult> CloseIncident(Guid incidentId, IncidentReasonRequest request, IMediator mediator, ICurrentTenant tenant, ICurrentUser currentUser, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId || currentUser.UserId is not { } actor) return Results.Unauthorized();
        Result<TrainingIncidentResponse> result=await mediator.Send(new CloseTrainingIncidentCommand(organizationId,new TrainingIncidentId(incidentId),request.OperationId,request.Reason,actor),ct); return result.IsSuccess?Results.Ok(result.Value):Failure(result.Error);
    }


    private static async Task<IResult> GetConsequences(Guid sessionId, ITrainingSessionCompletionConsequenceStore store, ICurrentTenant tenant, CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        IReadOnlyList<TrainingSessionConsequenceEnvelope> items = await store.GetBySessionAsync(organizationId, new TrainingSessionId(sessionId), ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> RetryConsequences(
        Guid sessionId,
        ITrainingSessionCompletionConsequenceStore store,
        ICurrentTenant tenant,
        IClock clock,
        CancellationToken ct)
    {
        if (tenant.OrganizationId is not { } organizationId) return Results.Unauthorized();
        await store.RequeueAsync(organizationId, new TrainingSessionId(sessionId), clock.UtcNow, ct);
        return Results.Accepted($"/api/training-delivery/sessions/{sessionId}/consequences");
    }

    private static IResult Failure(Error error) => Results.Problem(
        statusCode: error.Type == ErrorType.NotFound ? 404 : error.Type == ErrorType.Conflict ? 409 : 400,
        title: error.Code,
        detail: error.MessageKey);
    private sealed record StartSessionRequest(Guid OperationId, DateTimeOffset StartedAtUtc);

}


internal sealed record RecordAttendanceRequest(
    Guid OperationId,
    int Status,
    DateTimeOffset? ActualArrivalAtUtc,
    DateTimeOffset? ActualDepartureAtUtc,
    string? Reason,
    Guid? EvidenceDocumentId);

internal sealed record CorrectAttendanceRequest(
    Guid OperationId,
    int Status,
    DateTimeOffset? ActualArrivalAtUtc,
    DateTimeOffset? ActualDepartureAtUtc,
    string? Reason,
    Guid? EvidenceDocumentId,
    string? OverrideReason);

internal sealed record RecordInterventionRequest(Guid OperationId, int Type, int Severity, DateTimeOffset OccurredAtUtc, string Context, string Reason, Guid? RelatedCompetencyId, string? Outcome, string? InternalComment, string? SharedExplanation);
internal sealed record RecordObservationRequest(Guid OperationId, int Type, DateTimeOffset ObservedAtUtc, string Content, bool IsInternal);
internal sealed record RecordMarkerRequest(Guid OperationId, int Type, DateTimeOffset OccurredAtUtc, Guid? CompetencyId, string ShortNote, int Severity, decimal? Latitude, decimal? Longitude, bool CreatedOffline);
internal sealed record InterruptSessionRequest(Guid OperationId, int Reason, string? Description, DateTimeOffset InterruptedAtUtc);
internal sealed record ResumeSessionRequest(Guid OperationId, DateTimeOffset ResumedAtUtc);
internal sealed record RecordOdometerRequest(Guid OperationId, decimal OdometerKilometers, int Source, DateTimeOffset ObservedAtUtc);
internal sealed record RecordEnergyRequest(Guid OperationId, int Type, decimal? EnergyLevelPercent, decimal? Quantity, DateTimeOffset ObservedAtUtc, string? Note, bool CreatedOffline);


internal sealed record RecordCompetencyAssessmentRequest(
    Guid OperationId,
    Guid CompetencyId,
    string LevelCode,
    string? ObservedCriteria,
    string? Context,
    Guid? RelatedInterventionId,
    string? InternalComment,
    string? SharedComment,
    Guid? EvidenceDocumentId,
    DateTimeOffset AssessedAtUtc);

internal sealed record FinishTrainingSessionRequest(Guid OperationId, DateTimeOffset ActualEndAtUtc, decimal? EndEnergyLevelPercent);
internal sealed record CompleteTrainingSessionRequest(Guid OperationId, DateTimeOffset ActualEndAtUtc, string Summary, string? ObjectivesWorked, string? ObjectivesAchieved, string? NextObjective, string? InstructorComments);
internal sealed record SaveTrainingSessionReportDraftRequest(Guid OperationId, int ExpectedVersion, int LastCompletedStep, string? Summary, string? ObjectivesWorked, string? ObjectivesAchieved, string? NextObjective, string? SharedComment, string? InternalNote);
internal sealed record UpdateTrainingSessionNarrativeRequest(Guid OperationId, int ExpectedVersion, string? Content);
internal sealed record ReportVersionedOperationRequest(Guid OperationId, int ExpectedVersion);
internal sealed record SubmitTrainingSessionReportRequest(Guid OperationId, int ExpectedVersion, bool RequestSupervisorReview);
internal sealed record RequestTrainingSessionReportRevisionRequest(Guid OperationId, int ExpectedVersion, int Scenario, string FieldCode, string CurrentValue, string ProposedValue, string Reason, bool HasFinancialImpact, bool ApprovalRequired);
internal sealed record DecideTrainingSessionReportRevisionRequest(bool Approve, string? DecisionReason);
internal sealed record CancelTrainingSessionRequest(Guid OperationId, DateTimeOffset CancelledAtUtc, int Reason, string? ReasonDetails, int BillingDecision, int CreditDecision, decimal? PartialCreditQuantity, int ProviderCompensationDecision, string? DecisionReason);

internal sealed record IncidentParticipantRequest(int Type, Guid? ReferenceId, string? Label);
internal sealed record ReportIncidentRequest(Guid OperationId, int IncidentType, int Severity, DateTimeOffset OccurredAtUtc, string Description, string ImmediateActions, IReadOnlyCollection<IncidentParticipantRequest>? AdditionalParticipants);
internal sealed record AddIncidentEvidenceRequest(Guid OperationId, Guid DocumentId, string EvidenceType, string? Description);
internal sealed record IncidentReasonRequest(Guid OperationId, string? Reason);
internal sealed record IncidentResolutionRequest(Guid OperationId, string Resolution);
