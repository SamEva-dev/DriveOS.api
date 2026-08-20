using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Modules.SchedulingCapacity.Application.Availability;
using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Application.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Application.WaitingList;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.SchedulingCapacity.Application.Travel;
using DriveOS.Modules.SchedulingCapacity.Application.Capacity;
using DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

namespace DriveOS.Api.Endpoints.SchedulingCapacity;

internal static class SchedulingCapacityEndpoints
{
    internal static IEndpointRouteBuilder MapSchedulingCapacityEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/scheduling").WithTags("Scheduling & Capacity");

        group.MapGet("/resources", ListResources).RequireAuthorization("Scheduling.Resources.Read");
        group.MapGet("/resources/{resourceId:guid}", GetResource).RequireAuthorization("Scheduling.Resources.Read");
        group.MapPost("/resources", CreateResource).RequireAuthorization("Scheduling.Resources.Manage");
        group.MapPut("/resources/{resourceId:guid}", UpdateResource).RequireAuthorization("Scheduling.Resources.Manage");
        group.MapPost("/resources/{resourceId:guid}/restrict", RestrictResource).RequireAuthorization("Scheduling.Resources.Manage");
        group.MapPost("/resources/{resourceId:guid}/unavailable", MarkUnavailable).RequireAuthorization("Scheduling.Resources.Manage");
        group.MapPost("/resources/{resourceId:guid}/activate", ActivateResource).RequireAuthorization("Scheduling.Resources.Manage");
        group.MapPost("/resources/{resourceId:guid}/archive", ArchiveResource).RequireAuthorization("Scheduling.Resources.Manage");

        group.MapGet("/resources/{resourceId:guid}/availability-plans", ListAvailabilityPlans).RequireAuthorization("Scheduling.Availability.Read");
        group.MapGet("/availability-plans/{planId:guid}", GetAvailabilityPlan).RequireAuthorization("Scheduling.Availability.Read");
        group.MapPost("/resources/{resourceId:guid}/availability-plans", CreateAvailabilityPlan).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapPost("/availability-plans/{planId:guid}/rules", AddRule).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapDelete("/availability-plans/{planId:guid}/rules/{ruleId:guid}", RemoveRule).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapPost("/availability-plans/{planId:guid}/exceptions", AddException).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapDelete("/availability-plans/{planId:guid}/exceptions/{exceptionId:guid}", RemoveException).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapPut("/availability-plans/{planId:guid}/preferences", UpdateAvailabilityPreferences).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapPost("/availability-plans/{planId:guid}/activate", ActivateAvailabilityPlan).RequireAuthorization("Scheduling.Availability.Manage");
        group.MapPost("/availability-plans/{planId:guid}/archive", ArchiveAvailabilityPlan).RequireAuthorization("Scheduling.Availability.Manage");

        group.MapGet("/bookings", ListBookings).RequireAuthorization("Scheduling.Bookings.Read");
        group.MapGet("/bookings/{bookingId:guid}", GetBooking).RequireAuthorization("Scheduling.Bookings.Read");
        group.MapPost("/bookings", CreateBooking).RequireAuthorization("Scheduling.Bookings.Create");
        group.MapPost("/bookings/{bookingId:guid}/conflicts", CheckBookingConflicts).RequireAuthorization("Scheduling.Bookings.Read");
        group.MapPost("/bookings/{bookingId:guid}/hold", HoldBookingSlot).RequireAuthorization("Scheduling.Bookings.Reserve");
        group.MapPost("/bookings/{bookingId:guid}/reserve", ReserveBooking).RequireAuthorization("Scheduling.Bookings.Reserve");
        group.MapPost("/bookings/{bookingId:guid}/confirm", ConfirmBooking).RequireAuthorization("Scheduling.Bookings.Confirm");
        group.MapPost("/bookings/{bookingId:guid}/reschedule/preview", PreviewRescheduleBooking).RequireAuthorization("Scheduling.Bookings.Reschedule");
        group.MapPost("/bookings/{bookingId:guid}/reschedule", RescheduleBooking).RequireAuthorization("Scheduling.Bookings.Reschedule");
        group.MapPost("/bookings/{bookingId:guid}/cancel/preview", PreviewCancelBooking).RequireAuthorization("Scheduling.Bookings.Cancel");
        group.MapPost("/bookings/{bookingId:guid}/cancel", CancelBooking).RequireAuthorization("Scheduling.Bookings.Cancel");
        group.MapPost("/bookings/{bookingId:guid}/cancel/override", OverrideCancelBooking).RequireAuthorization("Scheduling.Bookings.CancelOverride");
        group.MapPost("/bookings/{bookingId:guid}/attendance", RecordBookingAttendance).RequireAuthorization("Scheduling.Attendance.Record");
        group.MapPost("/bookings/{bookingId:guid}/attendance/correct", CorrectBookingAttendance).RequireAuthorization("Scheduling.Attendance.UpdateWithinWindow");
        group.MapPost("/bookings/{bookingId:guid}/attendance/override", OverrideBookingAttendance).RequireAuthorization("Scheduling.Attendance.Override");

        group.MapGet("/recurrences", ListRecurrences).RequireAuthorization("Scheduling.Bookings.Read");
        group.MapGet("/recurrences/{seriesId:guid}", GetRecurrence).RequireAuthorization("Scheduling.Bookings.Read");
        group.MapGet("/recurrences/{seriesId:guid}/preview", PreviewRecurrence).RequireAuthorization("Scheduling.Bookings.Read");
        group.MapPost("/recurrences", CreateRecurrence).RequireAuthorization("Scheduling.Recurrence.Create");
        group.MapPost("/recurrences/{seriesId:guid}/generate", GenerateRecurrence).RequireAuthorization("Scheduling.Recurrence.Update");
        group.MapPost("/recurrences/{seriesId:guid}/occurrences/{occurrenceId:guid}/cancel", CancelRecurrenceOccurrence).RequireAuthorization("Scheduling.Recurrence.Cancel");
        group.MapPost("/recurrences/{seriesId:guid}/occurrences/{occurrenceId:guid}/reschedule", RescheduleRecurrenceOccurrence).RequireAuthorization("Scheduling.Recurrence.Update");
        group.MapPut("/recurrences/{seriesId:guid}/future-rule", ChangeFutureRecurrenceRule).RequireAuthorization("Scheduling.Recurrence.Update");
        group.MapPost("/recurrences/{seriesId:guid}/cancel", CancelRecurrenceSeries).RequireAuthorization("Scheduling.Recurrence.Cancel");

        group.MapGet("/conflicts", ListSchedulingConflicts).RequireAuthorization("Scheduling.Conflicts.Read");
        group.MapGet("/conflicts/{conflictId:guid}", GetSchedulingConflict).RequireAuthorization("Scheduling.Conflicts.Read");
        group.MapPost("/conflicts/scan/{bookingId:guid}", RefreshSchedulingConflicts).RequireAuthorization("Scheduling.Conflicts.Read");
        group.MapPost("/conflicts/{conflictId:guid}/resolve", ResolveSchedulingConflict).RequireAuthorization("Scheduling.Conflicts.Resolve");
        group.MapPost("/conflicts/{conflictId:guid}/override", OverrideSchedulingConflict).RequireAuthorization("Scheduling.Conflicts.Override");

        group.MapGet("/waiting-list", ListWaitingList).RequireAuthorization("Scheduling.WaitingList.Read");
        group.MapGet("/waiting-list/{entryId:guid}", GetWaitingListEntry).RequireAuthorization("Scheduling.WaitingList.Read");
        group.MapPost("/waiting-list", CreateWaitingListEntry).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPut("/waiting-list/{entryId:guid}/preferences", UpdateWaitingListPreferences).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPost("/waiting-list/match", MatchWaitingList).RequireAuthorization("Scheduling.WaitingList.Read");
        group.MapPost("/waiting-list/{entryId:guid}/proposals", ProposeWaitingListSlot).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPost("/waiting-list/{entryId:guid}/proposals/{proposalId:guid}/hold", HoldWaitingListProposal).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPost("/waiting-list/{entryId:guid}/proposals/{proposalId:guid}/accept", AcceptWaitingListProposal).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPost("/waiting-list/{entryId:guid}/proposals/{proposalId:guid}/fulfill", FulfillWaitingListEntry).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPost("/waiting-list/{entryId:guid}/proposals/{proposalId:guid}/decline", DeclineWaitingListProposal).RequireAuthorization("Scheduling.WaitingList.Manage");
        group.MapPost("/waiting-list/{entryId:guid}/cancel", CancelWaitingListEntry).RequireAuthorization("Scheduling.WaitingList.Manage");

        group.MapPost("/travel/evaluate", EvaluateTravel).RequireAuthorization("Scheduling.Travel.Read");
        group.MapPost("/slot-search", SearchSlots).RequireAuthorization("Scheduling.SlotSearch");
        group.MapGet("/capacity", GetCapacityForecast).RequireAuthorization("Scheduling.Capacity.Read");
        group.MapGet("/capacity/forecast", GetCapacityForecast).RequireAuthorization("Scheduling.Capacity.Forecast");
        group.MapPost("/capacity/scenarios", SimulateCapacityScenario).RequireAuthorization("Scheduling.Capacity.Scenarios.Create");
        group.MapPost("/replacements/instructor/suggestions", SuggestInstructorReplacements).RequireAuthorization("Scheduling.InstructorReplacement.Read");
        group.MapPost("/replacements/instructor/preview", PreviewInstructorReplacement).RequireAuthorization("Scheduling.InstructorReplacement.Read");
        group.MapPost("/replacements/instructor/apply", ApplyInstructorReplacement).RequireAuthorization("Scheduling.InstructorReplacement.Assign");
        group.MapPost("/replacements/vehicle/suggestions", SuggestVehicleReplacements).RequireAuthorization("Scheduling.VehicleReplacement.Read");
        group.MapPost("/replacements/vehicle/preview", PreviewVehicleReplacement).RequireAuthorization("Scheduling.VehicleReplacement.Read");
        group.MapPost("/replacements/vehicle/apply", ApplyVehicleReplacement).RequireAuthorization("Scheduling.VehicleReplacement.Assign");
        return app;
    }

    private static IResult Failure(Error error) => Results.Problem(
        statusCode: error.Type == ErrorType.NotFound ? StatusCodes.Status404NotFound : error.Type == ErrorType.Conflict ? StatusCodes.Status409Conflict : StatusCodes.Status400BadRequest,
        title: error.Code,
        detail: error.MessageKey);

    private static bool TryOrganization(ICurrentTenant tenant, out OrganizationId organizationId)
    {
        organizationId = default;
        if (tenant.OrganizationId is not { } id) return false;
        organizationId = id;
        return true;
    }

    private static async Task<IResult> ListResources(int? resourceType, Guid? branchId, ICalendarResourceReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        CalendarResourceType? type = resourceType.HasValue && Enum.IsDefined(typeof(CalendarResourceType), resourceType.Value) ? (CalendarResourceType)resourceType.Value : null;
        return Results.Ok(await service.ListAsync(organizationId, type, branchId.HasValue ? new BranchId(branchId.Value) : null, ct));
    }

    private static async Task<IResult> GetResource(Guid resourceId, ICalendarResourceReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        var response = await service.GetAsync(organizationId, new CalendarResourceId(resourceId), ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> CreateResource(CreateCalendarResourceRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<CalendarResourceId> result = await mediator.Send(new CreateCalendarResourceCommand(organizationId, request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null, request.ResourceType, request.ExternalResourceId, request.DisplayName, request.Capacity, request.TimeZoneId), ct);
        return result.IsSuccess ? Results.Created($"/api/scheduling/resources/{result.Value.Value}", new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> UpdateResource(Guid resourceId, UpdateCalendarResourceRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new UpdateCalendarResourceCommand(organizationId,new(resourceId),request.BranchId.HasValue?new BranchId(request.BranchId.Value):null,request.DisplayName,request.Capacity,request.TimeZoneId),ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> RestrictResource(Guid resourceId, ReasonRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) => await Mutate(mediator,tenant,new RestrictCalendarResourceCommand(tenant.OrganizationId ?? default,new(resourceId),request.Reason),ct);
    private static async Task<IResult> MarkUnavailable(Guid resourceId, OptionalReasonRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) => await Mutate(mediator,tenant,new MarkCalendarResourceUnavailableCommand(tenant.OrganizationId ?? default,new(resourceId),request.Reason),ct);
    private static async Task<IResult> ActivateResource(Guid resourceId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) => await Mutate(mediator,tenant,new ActivateCalendarResourceCommand(tenant.OrganizationId ?? default,new(resourceId)),ct);
    private static async Task<IResult> ArchiveResource(Guid resourceId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) => await Mutate(mediator,tenant,new ArchiveCalendarResourceCommand(tenant.OrganizationId ?? default,new(resourceId)),ct);
    private static async Task<IResult> Mutate(IMediator mediator, ICurrentTenant tenant, DriveOS.Application.Abstractions.Messaging.ICommand command, CancellationToken ct) { if(!TryOrganization(tenant,out _))return Results.Unauthorized(); Result r=await mediator.Send(command,ct); return r.IsSuccess?Results.NoContent():Failure(r.Error); }

    private static async Task<IResult> ListAvailabilityPlans(Guid resourceId, IAvailabilityPlanReadService service, ICurrentTenant tenant, CancellationToken ct) { if(!TryOrganization(tenant,out var o))return Results.Unauthorized(); return Results.Ok(await service.ListForResourceAsync(o,new(resourceId),ct)); }
    private static async Task<IResult> GetAvailabilityPlan(Guid planId, IAvailabilityPlanReadService service, ICurrentTenant tenant, CancellationToken ct) { if(!TryOrganization(tenant,out var o))return Results.Unauthorized(); var r=await service.GetAsync(o,new(planId),ct); return r is null?Results.NotFound():Results.Ok(r); }
    private static async Task<IResult> CreateAvailabilityPlan(Guid resourceId, CreateAvailabilityPlanRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) { if(!TryOrganization(tenant,out var o))return Results.Unauthorized(); Result<AvailabilityPlanId> r=await mediator.Send(new CreateAvailabilityPlanCommand(o,new(resourceId),request.EffectiveFrom,request.EffectiveTo),ct); return r.IsSuccess?Results.Created($"/api/scheduling/availability-plans/{r.Value.Value}",new{id=r.Value.Value}):Failure(r.Error); }
    private static async Task<IResult> AddRule(Guid planId, AddAvailabilityRuleRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<AvailabilityRuleId> result = await mediator.Send(new AddAvailabilityRuleCommand(
            organizationId,
            new AvailabilityPlanId(planId),
            request.DayOfWeek,
            request.StartTime,
            request.EndTime,
            request.Capacity,
            request.Type,
            request.Source,
            request.Priority,
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            request.TrainingCategory,
            request.ServiceArea), ct);
        return result.IsSuccess ? Results.Ok(new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> RemoveRule(Guid planId, Guid ruleId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new RemoveAvailabilityRuleCommand(organizationId, new AvailabilityPlanId(planId), new AvailabilityRuleId(ruleId)), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> AddException(Guid planId, AddAvailabilityExceptionRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<AddAvailabilityExceptionResult> result = await mediator.Send(new AddAvailabilityExceptionCommand(
            organizationId,
            new AvailabilityPlanId(planId),
            request.Date,
            request.StartTime,
            request.EndTime,
            request.Type,
            request.Capacity,
            request.Reason,
            request.Source,
            request.Priority), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> RemoveException(Guid planId, Guid exceptionId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new RemoveAvailabilityExceptionCommand(organizationId, new AvailabilityPlanId(planId), new AvailabilityExceptionId(exceptionId)), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> UpdateAvailabilityPreferences(Guid planId, UpdateAvailabilityPreferencesRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new UpdateAvailabilityPreferencesCommand(
            organizationId,
            new AvailabilityPlanId(planId),
            request.PreferredMeetingPoint,
            request.MaximumTravelDistanceKm,
            request.MinimumNoticeMinutes,
            request.TrainingFrequencyPerWeek,
            request.PreferredInstructorId.HasValue ? new UserId(request.PreferredInstructorId.Value) : null,
            request.IntensiveRhythm,
            request.OneTimeGeolocationAllowed), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }
    private static async Task<IResult> ActivateAvailabilityPlan(Guid planId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) => await Mutate(mediator,tenant,new ActivateAvailabilityPlanCommand(tenant.OrganizationId ?? default,new(planId)),ct);
    private static async Task<IResult> ArchiveAvailabilityPlan(Guid planId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct) => await Mutate(mediator,tenant,new ArchiveAvailabilityPlanCommand(tenant.OrganizationId ?? default,new(planId)),ct);

    private static async Task<IResult> ListBookings(Guid? branchId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, IBookingReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        return Results.Ok(await service.ListAsync(organizationId, branchId.HasValue ? new BranchId(branchId.Value) : null, fromUtc, toUtc, ct));
    }

    private static async Task<IResult> GetBooking(Guid bookingId, IBookingReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        BookingResponse? response = await service.GetAsync(organizationId, new BookingId(bookingId), ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> CreateBooking(CreateBookingRequest request, HttpRequest httpRequest, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        string idempotencyKey = httpRequest.Headers["Idempotency-Key"].ToString();
        if (string.IsNullOrWhiteSpace(idempotencyKey)) return Failure(BookingErrors.InvalidCreationIdempotency);

        Result<BookingId> result = await mediator.Send(new CreateBookingCommand(
            organizationId,
            idempotencyKey,
            request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
            request.BookingType,
            request.StartAtUtc,
            request.EndAtUtc,
            request.Title,
            request.TrainingPathId,
            request.TrainingCategory,
            request.Objectives,
            request.MeetingPoint,
            request.PricingReference,
            request.CreditReservation?.TrainingCreditAccountId,
            request.CreditReservation?.Quantity,
            request.Notes,
            request.NotificationPolicy,
            request.Resources.Select(x => new CreateBookingResourceRequest(x.CalendarResourceId, x.Quantity)).ToArray(),
            request.Participants.Select(x => new CreateBookingParticipantRequest(x.ParticipantType, x.ExternalParticipantId)).ToArray()), ct);
        return result.IsSuccess ? Results.Created($"/api/scheduling/bookings/{result.Value.Value}", new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> CheckBookingConflicts(Guid bookingId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingConflictCheckResponse> result = await mediator.Send(new CheckBookingConflictsCommand(organizationId, new BookingId(bookingId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> HoldBookingSlot(Guid bookingId, SlotHoldRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingConflictCheckResponse> result = await mediator.Send(
            new HoldBookingSlotCommand(organizationId, new BookingId(bookingId), request.DurationMinutes), ct);
        if (result.IsFailure) return Failure(result.Error);
        return result.Value.IsConflictFree ? Results.Ok(result.Value) : Results.Conflict(result.Value);
    }

    private static async Task<IResult> ReserveBooking(Guid bookingId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingConflictCheckResponse> result = await mediator.Send(new ReserveBookingCommand(organizationId, new BookingId(bookingId)), ct);
        if (result.IsFailure) return Failure(result.Error);
        return result.Value.IsConflictFree ? Results.Ok(result.Value) : Results.Conflict(result.Value);
    }

    private static async Task<IResult> ConfirmBooking(Guid bookingId, IMediator mediator, ICurrentTenant tenant, ICurrentUser user, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        if (user.UserId is null) return Results.Unauthorized();
        Result<BookingConflictCheckResponse> result = await mediator.Send(
            new ConfirmBookingCommand(organizationId, new BookingId(bookingId), user.UserId.Value),
            ct);
        if (result.IsFailure) return Failure(result.Error);
        return result.Value.IsConflictFree ? Results.Ok(result.Value) : Results.Conflict(result.Value);
    }

    private static async Task<IResult> PreviewRescheduleBooking(Guid bookingId, RescheduleBookingRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingRescheduleImpactResponse> result = await mediator.Send(
            new PreviewRescheduleBookingCommand(
                organizationId,
                new BookingId(bookingId),
                request.OperationId,
                request.StartAtUtc,
                request.EndAtUtc,
                request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
                request.Resources?.Select(x => new BookingRescheduleResourceRequest(x.CalendarResourceId, x.Quantity)).ToArray(),
                request.Reason),
            ct);
        if (result.IsFailure) return Failure(result.Error);
        return result.Value.CanConfirm ? Results.Ok(result.Value) : Results.Conflict(result.Value);
    }

    private static async Task<IResult> RescheduleBooking(Guid bookingId, RescheduleBookingRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingRescheduleImpactResponse> result = await mediator.Send(
            new RescheduleBookingCommand(
                organizationId,
                new BookingId(bookingId),
                request.OperationId,
                request.StartAtUtc,
                request.EndAtUtc,
                request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null,
                request.Resources?.Select(x => new BookingRescheduleResourceRequest(x.CalendarResourceId, x.Quantity)).ToArray(),
                request.Reason),
            ct);
        if (result.IsFailure) return Failure(result.Error);
        return result.Value.CanConfirm ? Results.Ok(result.Value) : Results.Conflict(result.Value);
    }

    private static async Task<IResult> ListRecurrences(IRecurrenceSeriesReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        return Results.Ok(await service.ListAsync(organizationId, ct));
    }

    private static async Task<IResult> GetRecurrence(Guid seriesId, IRecurrenceSeriesReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        RecurrenceSeriesResponse? response = await service.GetAsync(organizationId, new RecurrenceSeriesId(seriesId), ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> PreviewRecurrence(Guid seriesId, IRecurrencePreviewService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        RecurrencePreviewResponse? response = await service.PreviewAsync(organizationId, new RecurrenceSeriesId(seriesId), ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> CreateRecurrence(CreateRecurrenceSeriesRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<RecurrenceSeriesId> result = await mediator.Send(new CreateRecurrenceSeriesCommand(organizationId, request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null, request.TargetType, request.Frequency, request.Interval, request.StartDate, request.EndDate, request.OccurrenceCount, request.DaysOfWeek, request.LocalTime, request.DurationMinutes, request.TimeZoneId, request.Title, request.ResourceSelectionPolicy, request.Resources.Select(x => new CreateRecurrenceResourceRequest(x.CalendarResourceId, x.Quantity)).ToArray()), ct);
        return result.IsSuccess ? Results.Created($"/api/scheduling/recurrences/{result.Value.Value}", new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> GenerateRecurrence(Guid seriesId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<int> result = await mediator.Send(new GenerateRecurrenceOccurrencesCommand(organizationId, new RecurrenceSeriesId(seriesId)), ct);
        return result.IsSuccess ? Results.Ok(new { generated = result.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> CancelRecurrenceOccurrence(Guid seriesId, Guid occurrenceId, RecurrenceReasonRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new CancelRecurrenceOccurrenceCommand(organizationId, new RecurrenceSeriesId(seriesId), new RecurrenceOccurrenceId(occurrenceId), request.Reason), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> RescheduleRecurrenceOccurrence(Guid seriesId, Guid occurrenceId, RescheduleRecurrenceOccurrenceRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new RescheduleRecurrenceOccurrenceCommand(organizationId, new RecurrenceSeriesId(seriesId), new RecurrenceOccurrenceId(occurrenceId), request.StartAtUtc, request.EndAtUtc, request.Reason), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> ChangeFutureRecurrenceRule(Guid seriesId, ChangeFutureRecurrenceRuleRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new ChangeFutureRecurrenceRuleCommand(organizationId, new RecurrenceSeriesId(seriesId), request.ApplyFrom, request.Frequency, request.Interval, request.EndDate, request.OccurrenceCount, request.DaysOfWeek, request.LocalTime, request.DurationMinutes), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> CancelRecurrenceSeries(Guid seriesId, RecurrenceReasonRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new CancelRecurrenceSeriesCommand(organizationId, new RecurrenceSeriesId(seriesId), request.Reason), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> ListSchedulingConflicts(int? status, int? priority, Guid? bookingId, ISchedulingConflictReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        return Results.Ok(await service.ListAsync(organizationId, status, priority, bookingId.HasValue ? new BookingId(bookingId.Value) : null, ct));
    }

    private static async Task<IResult> GetSchedulingConflict(Guid conflictId, ISchedulingConflictReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        SchedulingConflictResponse? response = await service.GetAsync(organizationId, new SchedulingConflictId(conflictId), ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> RefreshSchedulingConflicts(Guid bookingId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<SchedulingConflictScanResponse> result = await mediator.Send(new RefreshSchedulingConflictsCommand(organizationId, new BookingId(bookingId)), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> ResolveSchedulingConflict(Guid conflictId, ResolveSchedulingConflictRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new ResolveSchedulingConflictCommand(organizationId, new SchedulingConflictId(conflictId), request.Resolution, request.Reason), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> OverrideSchedulingConflict(Guid conflictId, OverrideSchedulingConflictRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new OverrideSchedulingConflictCommand(organizationId, new SchedulingConflictId(conflictId), request.Reason, request.Risk, request.ExpiresAtUtc), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> ListWaitingList(int? status, Guid? studentId, IWaitingListReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        return Results.Ok(await service.ListAsync(organizationId, status, studentId, ct));
    }

    private static async Task<IResult> GetWaitingListEntry(Guid entryId, IWaitingListReadService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        WaitingListEntryResponse? response = await service.GetAsync(organizationId, new WaitingListEntryId(entryId), ct);
        return response is null ? Results.NotFound() : Results.Ok(response);
    }

    private static async Task<IResult> CreateWaitingListEntry(CreateWaitingListEntryRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        if (request.Priority is null) return Results.BadRequest(new { code = "SchedulingCapacity.WaitingList.PriorityRequired" });
        if (!Enum.IsDefined(typeof(DriveOS.Modules.SchedulingCapacity.Domain.Bookings.BookingType), request.RequestedSessionType))
            return Results.BadRequest(new { code = "SchedulingCapacity.WaitingList.InvalidSessionType" });
        var priority = new DriveOS.Modules.SchedulingCapacity.Domain.WaitingList.WaitingListPriorityInput(request.Priority!.ExamAtUtc, request.Priority!.HasNoFutureSession, request.Priority!.InterruptionDays, request.Priority!.PedagogicalUrgencyLevel, request.Priority!.SchoolCancellation, request.Priority!.LimitedAvailability, request.Priority!.RegulatoryPriority, request.Priority!.CommercialPriority, request.Priority!.ManualAdjustment, request.Priority!.ManualAdjustmentReason);
        Result<WaitingListEntryId> result = await mediator.Send(new CreateWaitingListEntryCommand(organizationId, new PersonId(request.StudentId), (DriveOS.Modules.SchedulingCapacity.Domain.Bookings.BookingType)request.RequestedSessionType, request.PreferredFromUtc, request.PreferredToUtc, request.DurationMinutes, request.PreferredBranchId.HasValue ? new BranchId(request.PreferredBranchId.Value) : null, request.PreferredInstructorId.HasValue ? new UserId(request.PreferredInstructorId.Value) : null, priority, request.Reason, request.ExpiresAtUtc), ct);
        return result.IsSuccess ? Results.Created($"/api/scheduling/waiting-list/{result.Value.Value}", new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> UpdateWaitingListPreferences(Guid entryId, UpdateWaitingListPreferencesRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new UpdateWaitingListPreferencesCommand(organizationId, new WaitingListEntryId(entryId), request.PreferredFromUtc, request.PreferredToUtc, request.PreferredBranchId.HasValue ? new BranchId(request.PreferredBranchId.Value) : null, request.PreferredInstructorId.HasValue ? new UserId(request.PreferredInstructorId.Value) : null, request.ExpiresAtUtc), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> MatchWaitingList(MatchWaitingListRequest request, IWaitingListMatchingService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        IReadOnlyCollection<WaitingListMatchCandidateResponse> response = await service.MatchAsync(organizationId, request.StartAtUtc, request.EndAtUtc, request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null, request.InstructorId.HasValue ? new UserId(request.InstructorId.Value) : null, request.MaxResults ?? 20, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> ProposeWaitingListSlot(Guid entryId, ProposeWaitingListSlotRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<WaitingListProposalId> result = await mediator.Send(new ProposeWaitingListSlotCommand(organizationId, new WaitingListEntryId(entryId), request.StartAtUtc, request.EndAtUtc, request.BranchId.HasValue ? new BranchId(request.BranchId.Value) : null, request.InstructorId.HasValue ? new UserId(request.InstructorId.Value) : null, request.ExpiresAtUtc), ct);
        return result.IsSuccess ? Results.Created($"/api/scheduling/waiting-list/{entryId}/proposals/{result.Value.Value}", new { id = result.Value.Value }) : Failure(result.Error);
    }

    private static async Task<IResult> HoldWaitingListProposal(Guid entryId, Guid proposalId, HoldWaitingListProposalRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new HoldWaitingListProposalCommand(organizationId, new WaitingListEntryId(entryId), new WaitingListProposalId(proposalId), request.HeldUntilUtc), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> AcceptWaitingListProposal(Guid entryId, Guid proposalId, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new AcceptWaitingListProposalCommand(organizationId, new WaitingListEntryId(entryId), new WaitingListProposalId(proposalId)), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> FulfillWaitingListEntry(Guid entryId, Guid proposalId, FulfillWaitingListEntryRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new FulfillWaitingListEntryCommand(organizationId, new WaitingListEntryId(entryId), new WaitingListProposalId(proposalId), new BookingId(request.BookingId)), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> DeclineWaitingListProposal(Guid entryId, Guid proposalId, WaitingListReasonRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new DeclineWaitingListProposalCommand(organizationId, new WaitingListEntryId(entryId), new WaitingListProposalId(proposalId), request.Reason), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> CancelWaitingListEntry(Guid entryId, WaitingListReasonRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result result = await mediator.Send(new CancelWaitingListEntryCommand(organizationId, new WaitingListEntryId(entryId), request.Reason ?? string.Empty), ct);
        return result.IsSuccess ? Results.NoContent() : Failure(result.Error);
    }

    private static async Task<IResult> PreviewCancelBooking(Guid bookingId, PreviewCancelBookingRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingCancellationPreviewResponse> result = await mediator.Send(new PreviewCancelBookingCommand(
            organizationId, new BookingId(bookingId), request.Initiator, request.InitiatorId, request.ReasonCode, request.ReasonDetails), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> CancelBooking(
        Guid bookingId,
        CancelBookingRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        if (currentUser.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Guid? trustedInitiatorId = TrustedCancellationInitiatorId(request.Initiator, currentUser.UserId.Value);
        Result<BookingCancellationResponse> result = await mediator.Send(new CancelBookingCommand(
            organizationId, new BookingId(bookingId), request.OperationId, request.Initiator, trustedInitiatorId,
            request.ReasonCode, request.ReasonDetails, request.NotificationDecision, false, null), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> OverrideCancelBooking(
        Guid bookingId,
        OverrideCancelBookingRequest request,
        IMediator mediator,
        ICurrentTenant tenant,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        if (currentUser.UserId is null) return Results.Problem(statusCode: 401, title: "errors.currentUser.required");

        Guid? trustedInitiatorId = TrustedCancellationInitiatorId(request.Initiator, currentUser.UserId.Value);
        Result<BookingCancellationResponse> result = await mediator.Send(new CancelBookingCommand(
            organizationId, new BookingId(bookingId), request.OperationId, request.Initiator, trustedInitiatorId,
            request.ReasonCode, request.ReasonDetails, request.NotificationDecision, true, request.OverrideReason), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static Guid? TrustedCancellationInitiatorId(int initiator, UserId currentUserId) =>
        initiator is (int)CancellationInitiator.System or (int)CancellationInitiator.ForceMajeure
            ? null
            : currentUserId.Value;

    private static async Task<IResult> RecordBookingAttendance(Guid bookingId, BookingAttendanceRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingAttendanceResponse> result = await mediator.Send(new RecordBookingAttendanceCommand(
            organizationId, new BookingId(bookingId), request.OperationId, request.Status, request.ArrivalTimeUtc,
            request.DepartureTimeUtc, request.DelayMinutes, request.Reason, request.EvidenceDocumentId, request.FollowUpAction), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> CorrectBookingAttendance(Guid bookingId, BookingAttendanceRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingAttendanceResponse> result = await mediator.Send(new CorrectBookingAttendanceCommand(
            organizationId, new BookingId(bookingId), request.OperationId, request.Status, request.ArrivalTimeUtc,
            request.DepartureTimeUtc, request.DelayMinutes, request.Reason, request.EvidenceDocumentId, request.FollowUpAction, false, null), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static async Task<IResult> OverrideBookingAttendance(Guid bookingId, OverrideBookingAttendanceRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<BookingAttendanceResponse> result = await mediator.Send(new CorrectBookingAttendanceCommand(
            organizationId, new BookingId(bookingId), request.OperationId, request.Status, request.ArrivalTimeUtc,
            request.DepartureTimeUtc, request.DelayMinutes, request.Reason, request.EvidenceDocumentId, request.FollowUpAction, true, request.OverrideReason), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }
    private static async Task<IResult> SuggestInstructorReplacements(InstructorReplacementSuggestionRequest request, IInstructorReplacementService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        if (request.BookingIds is null || request.BookingIds.Count == 0 || request.PreviousInstructorId == Guid.Empty) return Results.BadRequest();
        IReadOnlyCollection<InstructorReplacementSuggestionResponse> response = await service.SuggestAsync(
            organizationId, new UserId(request.PreviousInstructorId), request.BookingIds.Select(x => new BookingId(x)).ToArray(), request.TrainingCategory, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> PreviewInstructorReplacement(InstructorReplacementRequest request, IInstructorReplacementService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        InstructorReplacementPreviewResponse? response = await service.PreviewAsync(
            organizationId, request.OperationId, new UserId(request.PreviousInstructorId), new UserId(request.ReplacementInstructorId),
            request.Mode, request.BookingIds.Select(x => new BookingId(x)).ToArray(), request.TrainingCategory, request.AccessExpiresAtUtc, ct);
        if (response is null) return Results.BadRequest();
        return response.CanConfirm ? Results.Ok(response) : Results.Conflict(response);
    }

    private static async Task<IResult> ApplyInstructorReplacement(InstructorReplacementRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<InstructorReplacementApplyResponse> result = await mediator.Send(new ApplyInstructorReplacementCommand(
            organizationId, request.OperationId, new UserId(request.PreviousInstructorId), new UserId(request.ReplacementInstructorId),
            request.Mode, request.BookingIds.Select(x => new BookingId(x)).ToArray(), request.TrainingCategory, request.Reason, request.AccessExpiresAtUtc), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }

    private static VehicleReplacementRequirements MapVehicleRequirements(VehicleReplacementRequirementsRequest request) => new(
        request.TrainingCategory, request.TransmissionType, request.DualControlRequired, request.RequiredAdaptations ?? [], request.EnergyType);

    private static async Task<IResult> SuggestVehicleReplacements(VehicleReplacementSuggestionRequest request, IVehicleReplacementService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        if (request.PreviousVehicleId == Guid.Empty || request.BookingIds is null || request.BookingIds.Count == 0) return Results.BadRequest();
        IReadOnlyCollection<VehicleReplacementSuggestionResponse> response = await service.SuggestAsync(organizationId, request.PreviousVehicleId,
            request.BookingIds.Select(x => new BookingId(x)).ToArray(), MapVehicleRequirements(request.Requirements), ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> PreviewVehicleReplacement(VehicleReplacementRequest request, IVehicleReplacementService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        VehicleReplacementPreviewResponse? response = await service.PreviewAsync(organizationId, request.OperationId, request.PreviousVehicleId, request.ReplacementVehicleId,
            request.Mode, request.BookingIds.Select(x => new BookingId(x)).ToArray(), MapVehicleRequirements(request.Requirements), ct);
        if (response is null) return Results.BadRequest();
        return response.CanConfirm ? Results.Ok(response) : Results.Conflict(response);
    }

    private static async Task<IResult> ApplyVehicleReplacement(VehicleReplacementRequest request, IMediator mediator, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        Result<VehicleReplacementApplyResponse> result = await mediator.Send(new ApplyVehicleReplacementCommand(organizationId, request.OperationId,
            request.PreviousVehicleId, request.ReplacementVehicleId, request.Mode, request.BookingIds.Select(x => new BookingId(x)).ToArray(),
            MapVehicleRequirements(request.Requirements), request.Reason), ct);
        return result.IsSuccess ? Results.Ok(result.Value) : Failure(result.Error);
    }


    private static async Task<IResult> GetCapacityForecast(
        int horizon,
        Guid? branchId,
        ICapacityForecastService service,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (!TryOrganization(tenant, out OrganizationId organizationId)) return Results.Unauthorized();
        if (!Enum.IsDefined(typeof(CapacityForecastHorizon), horizon))
            return Results.BadRequest(new { code = "Scheduling.Capacity.InvalidHorizon", message = "errors.schedulingCapacity.capacity.invalidHorizon" });
        BranchId? branch = branchId.HasValue ? new BranchId(branchId.Value) : null;
        CapacityForecastResponse response = await service.ForecastAsync(organizationId, (CapacityForecastHorizon)horizon, branch, cancellationToken);
        return Results.Ok(response);
    }

    private static async Task<IResult> SimulateCapacityScenario(
        CapacityScenarioRequest request,
        ICapacityForecastService service,
        ICurrentTenant tenant,
        CancellationToken cancellationToken)
    {
        if (!TryOrganization(tenant, out OrganizationId organizationId)) return Results.Unauthorized();
        try
        {
            CapacityScenarioResponse response = await service.SimulateAsync(organizationId, request, cancellationToken);
            return Results.Ok(response);
        }
        catch (CapacityForecastValidationException exception)
        {
            return Results.BadRequest(new
            {
                code = "Scheduling.Capacity.InvalidScenario",
                messageKey = exception.MessageKey
            });
        }
    }

    private static async Task<IResult> EvaluateTravel(
        EvaluateTravelRequest request,
        ITravelPlanningService travelPlanningService,
        Microsoft.AspNetCore.Authorization.IAuthorizationService authorizationService,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        bool usesPreciseLocation = request.Origin.Mode is not DriveOS.Modules.SchedulingCapacity.Domain.Travel.TravelLocationMode.ManualAddress
            || request.Destination.Mode is not DriveOS.Modules.SchedulingCapacity.Domain.Travel.TravelLocationMode.ManualAddress;
        if (usesPreciseLocation)
        {
            Microsoft.AspNetCore.Authorization.AuthorizationResult authorization = await authorizationService.AuthorizeAsync(
                httpContext.User,
                null,
                "Scheduling.Travel.PreciseLocation");
            if (!authorization.Succeeded)
                return Results.Forbid();
        }

        try
        {
            TravelEvaluationResponse response = await travelPlanningService.EvaluateAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (TravelPlanningException exception)
        {
            return Results.BadRequest(new
            {
                code = exception.Code,
                messageKey = exception.MessageKey,
                parameters = exception.Parameters
            });
        }
    }

    private static async Task<IResult> SearchSlots(SlotSearchRequest request, ISlotSearchService service, ICurrentTenant tenant, CancellationToken ct)
    {
        if (!TryOrganization(tenant, out var organizationId)) return Results.Unauthorized();
        try
        {
            return Results.Ok(await service.SearchAsync(organizationId, request, ct));
        }
        catch (SlotSearchValidationException exception)
        {
            return Results.BadRequest(new
            {
                code = "Scheduling.SlotSearch.InvalidRequest",
                messageKey = exception.MessageKey,
                parameters = exception.Parameters
            });
        }
    }

}

public sealed record CreateCalendarResourceRequest(Guid? BranchId, int ResourceType, Guid ExternalResourceId, string DisplayName, int Capacity, string TimeZoneId);
public sealed record UpdateCalendarResourceRequest(Guid? BranchId, string DisplayName, int Capacity, string TimeZoneId);
public sealed record ReasonRequest(string Reason);
public sealed record OptionalReasonRequest(string? Reason);
public sealed record CreateAvailabilityPlanRequest(DateOnly EffectiveFrom, DateOnly? EffectiveTo);
public sealed record AddAvailabilityRuleRequest(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Capacity,
    int Type,
    int Source,
    int Priority,
    Guid? BranchId,
    string? TrainingCategory,
    string? ServiceArea);
public sealed record AddAvailabilityExceptionRequest(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int Type,
    int? Capacity,
    string? Reason,
    int? Source,
    int? Priority);
public sealed record UpdateAvailabilityPreferencesRequest(
    string? PreferredMeetingPoint,
    decimal? MaximumTravelDistanceKm,
    int? MinimumNoticeMinutes,
    int? TrainingFrequencyPerWeek,
    Guid? PreferredInstructorId,
    bool IntensiveRhythm,
    bool OneTimeGeolocationAllowed);

public sealed record CreateBookingRequest(
    Guid? BranchId,
    int BookingType,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    string Title,
    Guid? TrainingPathId,
    string? TrainingCategory,
    string? Objectives,
    string? MeetingPoint,
    string? PricingReference,
    BookingCreditReservationBody? CreditReservation,
    string? Notes,
    int NotificationPolicy,
    IReadOnlyCollection<CreateBookingResourceBody> Resources,
    IReadOnlyCollection<CreateBookingParticipantBody> Participants);
public sealed record BookingCreditReservationBody(Guid TrainingCreditAccountId, decimal Quantity);
public sealed record CreateBookingResourceBody(Guid CalendarResourceId, int Quantity);
public sealed record SlotHoldRequest(int DurationMinutes);
public sealed record CreateBookingParticipantBody(int ParticipantType, Guid ExternalParticipantId);
public sealed record RescheduleBookingRequest(
    Guid OperationId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    Guid? BranchId,
    IReadOnlyCollection<BookingRescheduleResourceBody>? Resources,
    string Reason);
public sealed record BookingRescheduleResourceBody(Guid CalendarResourceId, int Quantity);
public sealed record PreviewCancelBookingRequest(int Initiator, Guid? InitiatorId, int ReasonCode, string? ReasonDetails);
public sealed record CancelBookingRequest(Guid OperationId, int Initiator, Guid? InitiatorId, int ReasonCode, string? ReasonDetails, int NotificationDecision);
public sealed record OverrideCancelBookingRequest(Guid OperationId, int Initiator, Guid? InitiatorId, int ReasonCode, string? ReasonDetails, int NotificationDecision, string OverrideReason);
public sealed record BookingAttendanceRequest(Guid OperationId, int Status, DateTimeOffset? ArrivalTimeUtc, DateTimeOffset? DepartureTimeUtc, int DelayMinutes, string? Reason, Guid? EvidenceDocumentId, int FollowUpAction);
public sealed record OverrideBookingAttendanceRequest(Guid OperationId, int Status, DateTimeOffset? ArrivalTimeUtc, DateTimeOffset? DepartureTimeUtc, int DelayMinutes, string? Reason, Guid? EvidenceDocumentId, int FollowUpAction, string OverrideReason);

public sealed record CreateRecurrenceSeriesRequest(Guid? BranchId, int TargetType, int Frequency, int Interval, DateOnly StartDate, DateOnly? EndDate, int? OccurrenceCount, IReadOnlyCollection<DayOfWeek> DaysOfWeek, TimeOnly LocalTime, int DurationMinutes, string TimeZoneId, string Title, int ResourceSelectionPolicy, IReadOnlyCollection<CreateRecurrenceResourceBody> Resources);
public sealed record CreateRecurrenceResourceBody(Guid CalendarResourceId, int Quantity);
public sealed record RecurrenceReasonRequest(string Reason);
public sealed record RescheduleRecurrenceOccurrenceRequest(DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, string Reason);
public sealed record ChangeFutureRecurrenceRuleRequest(DateOnly ApplyFrom, int Frequency, int Interval, DateOnly? EndDate, int? OccurrenceCount, IReadOnlyCollection<DayOfWeek> DaysOfWeek, TimeOnly LocalTime, int DurationMinutes);

public sealed record ResolveSchedulingConflictRequest(int Resolution, string Reason);
public sealed record OverrideSchedulingConflictRequest(string Reason, string Risk, DateTimeOffset ExpiresAtUtc);

public sealed record CreateWaitingListEntryRequest(Guid StudentId, int RequestedSessionType, DateTimeOffset PreferredFromUtc, DateTimeOffset PreferredToUtc, int DurationMinutes, Guid? PreferredBranchId, Guid? PreferredInstructorId, WaitingListPriorityRequest? Priority, string Reason, DateTimeOffset ExpiresAtUtc);
public sealed record WaitingListPriorityRequest(DateTimeOffset? ExamAtUtc, bool HasNoFutureSession, int InterruptionDays, int PedagogicalUrgencyLevel, bool SchoolCancellation, bool LimitedAvailability, bool RegulatoryPriority, bool CommercialPriority, int ManualAdjustment, string? ManualAdjustmentReason);
public sealed record UpdateWaitingListPreferencesRequest(DateTimeOffset PreferredFromUtc, DateTimeOffset PreferredToUtc, Guid? PreferredBranchId, Guid? PreferredInstructorId, DateTimeOffset ExpiresAtUtc);
public sealed record MatchWaitingListRequest(DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, Guid? BranchId, Guid? InstructorId, int? MaxResults);
public sealed record ProposeWaitingListSlotRequest(DateTimeOffset StartAtUtc, DateTimeOffset EndAtUtc, Guid? BranchId, Guid? InstructorId, DateTimeOffset ExpiresAtUtc);
public sealed record HoldWaitingListProposalRequest(DateTimeOffset HeldUntilUtc);
public sealed record FulfillWaitingListEntryRequest(Guid BookingId);
public sealed record WaitingListReasonRequest(string? Reason);

public sealed record InstructorReplacementSuggestionRequest(Guid PreviousInstructorId, IReadOnlyCollection<Guid> BookingIds, string TrainingCategory);
public sealed record InstructorReplacementRequest(Guid OperationId, Guid PreviousInstructorId, Guid ReplacementInstructorId, int Mode, IReadOnlyCollection<Guid> BookingIds, string TrainingCategory, string Reason, DateTimeOffset? AccessExpiresAtUtc);
public sealed record VehicleReplacementRequirementsRequest(string TrainingCategory, string? TransmissionType, bool DualControlRequired, IReadOnlyCollection<string>? RequiredAdaptations, string? EnergyType);
public sealed record VehicleReplacementSuggestionRequest(Guid PreviousVehicleId, IReadOnlyCollection<Guid> BookingIds, VehicleReplacementRequirementsRequest Requirements);
public sealed record VehicleReplacementRequest(Guid OperationId, Guid PreviousVehicleId, Guid ReplacementVehicleId, int Mode, IReadOnlyCollection<Guid> BookingIds, VehicleReplacementRequirementsRequest Requirements, string Reason);
