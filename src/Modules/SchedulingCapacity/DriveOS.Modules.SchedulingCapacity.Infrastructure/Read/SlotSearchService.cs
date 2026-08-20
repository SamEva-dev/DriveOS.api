using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.SlotSearch;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Read;

internal sealed class SlotSearchService(
    SchedulingCapacityDbContext dbContext,
    IBookingConflictAssessmentService conflictAssessmentService,
    ISlotSearchInstructorContextGateway instructorContextGateway) : ISlotSearchService
{
    private const int MaxResourceCandidatesPerType = 12;
    private const int MaxEvaluatedCandidates = 25_000;

    public async Task<SlotSearchResponse> SearchAsync(
        OrganizationId organizationId,
        SlotSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        BranchId? branchId = request.BranchId is { } branchGuid ? new BranchId(branchGuid) : null;
        var warnings = new List<string>();

        CalendarResource? student = await dbContext.CalendarResources.AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId
                && x.ResourceType == CalendarResourceType.Student
                && x.ExternalResourceId == request.StudentId
                && x.Status == CalendarResourceStatus.Active, cancellationToken);

        if (student is null)
        {
            warnings.Add("slotSearch.studentCalendarResourceMissing");
            return Empty(request, warnings);
        }

        List<CalendarResource> instructors = await Candidates(
            organizationId, branchId, CalendarResourceType.Instructor, request.PreferredInstructorId, cancellationToken);

        if (instructors.Count == 0)
        {
            warnings.Add("slotSearch.noInstructorCandidate");
            return Empty(request, warnings);
        }

        List<CalendarResource> vehicles = request.RequireVehicle
            ? await Candidates(organizationId, branchId, CalendarResourceType.Vehicle, request.PreferredVehicleId, cancellationToken)
            : [];

        if (request.RequireVehicle && vehicles.Count == 0)
        {
            warnings.Add("slotSearch.noVehicleCandidate");
            return Empty(request, warnings);
        }

        List<CalendarResource> rooms = request.RequireRoom
            ? await Candidates(organizationId, branchId, CalendarResourceType.Room, null, cancellationToken)
            : [];

        if (request.RequireRoom && rooms.Count == 0)
        {
            warnings.Add("slotSearch.noRoomCandidate");
            return Empty(request, warnings);
        }

        if (string.IsNullOrWhiteSpace(request.TrainingCategory))
            warnings.Add("slotSearch.qualificationRequiresTrainingCategory");
        if (request.PreferredInstructorId is null && !request.PreferContinuity)
            warnings.Add("slotSearch.continuityNotRequested");
        warnings.Add("slotSearch.geographicRoutingOptional");
        warnings.Add("slotSearch.costRequiresExternalContext");

        DateTimeOffset normalizedFrom = request.FromUtc.ToUniversalTime();
        DateTimeOffset normalizedTo = request.ToUtc.ToUniversalTime();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Booking[] loadBookings = await dbContext.Bookings.AsNoTracking()
            .Include(x => x.Resources)
            .Where(x => x.OrganizationId == organizationId
                && x.StartAtUtc < normalizedTo
                && x.EndAtUtc > normalizedFrom
                && (x.Status == BookingStatus.Reserved
                    || x.Status == BookingStatus.Confirmed
                    || (x.Status == BookingStatus.Tentative && x.HoldExpiresAtUtc > now)))
            .ToArrayAsync(cancellationToken);

        Dictionary<Guid, int> scheduledMinutes = BuildScheduledMinutes(loadBookings, normalizedFrom, normalizedTo);
        int minimumInstructorLoad = instructors.Select(x => scheduledMinutes.GetValueOrDefault(x.Id.Value)).DefaultIfEmpty(0).Min();
        int minimumVehicleLoad = vehicles.Select(x => scheduledMinutes.GetValueOrDefault(x.Id.Value)).DefaultIfEmpty(0).Min();

        var instructorContexts = new Dictionary<Guid, SlotSearchInstructorContext>();
        foreach (CalendarResource instructor in instructors)
        {
            if (string.IsNullOrWhiteSpace(request.TrainingCategory))
            {
                instructorContexts[instructor.Id.Value] = new SlotSearchInstructorContext(false, true, false, []);
                continue;
            }

            SlotSearchInstructorContext context = await instructorContextGateway.EvaluateAsync(
                organizationId,
                new PersonId(request.StudentId),
                new UserId(instructor.ExternalResourceId),
                branchId ?? instructor.BranchId,
                request.TrainingCategory.Trim(),
                cancellationToken);

            instructorContexts[instructor.Id.Value] = context;
        }

        var suggestions = new List<SlotSearchSuggestion>();
        int evaluated = 0;
        DateTimeOffset lastStart = normalizedTo.AddMinutes(-request.DurationMinutes);

        IEnumerable<CalendarResource?> vehicleOptions = request.RequireVehicle ? vehicles : new CalendarResource?[] { null }; ;
        IEnumerable<CalendarResource?> roomOptions = request.RequireRoom ? rooms : new CalendarResource?[] { null }; ;

        for (DateTimeOffset start = normalizedFrom; start <= lastStart; start = start.AddMinutes(request.StepMinutes))
        {
            DateTimeOffset end = start.AddMinutes(request.DurationMinutes);

            foreach (CalendarResource instructor in instructors)
            foreach (CalendarResource? vehicle in vehicleOptions)
            foreach (CalendarResource? room in roomOptions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (evaluated >= MaxEvaluatedCandidates)
                {
                    warnings.Add("slotSearch.evaluationLimitReached");
                    return BuildResponse(request, evaluated, suggestions, warnings);
                }
                evaluated++;

                SlotSearchInstructorContext instructorContext = instructorContexts[instructor.Id.Value];
                if (!instructorContext.IsEligible)
                    continue;

                Booking booking = Booking.Create(
                    BookingId.New(), organizationId, branchId,
                    (BookingType)request.BookingType, start, end, "slot-search").Value;

                booking.AddResource(BookingResourceId.New(), student.Id);
                booking.AddResource(BookingResourceId.New(), instructor.Id);
                if (vehicle is not null) booking.AddResource(BookingResourceId.New(), vehicle.Id);
                if (room is not null) booking.AddResource(BookingResourceId.New(), room.Id);
                booking.AddParticipant(BookingParticipantId.New(), BookingParticipantType.Student, request.StudentId);
                booking.AddParticipant(BookingParticipantId.New(), BookingParticipantType.Instructor, instructor.ExternalResourceId);

                BookingConflictAssessment assessment = await conflictAssessmentService.AssessAsync(booking, cancellationToken);
                if (!assessment.IsConflictFree) continue;

                int instructorLoad = scheduledMinutes.GetValueOrDefault(instructor.Id.Value);
                int vehicleLoad = vehicle is null ? 0 : scheduledMinutes.GetValueOrDefault(vehicle.Id.Value);

                int score = 100;
                var reasons = new List<string> { "slotSearch.reason.allBlockingConstraintsSatisfied" };
                var externalReviews = new List<string>();

                if (instructorContext.QualificationVerified)
                {
                    score += 12;
                    reasons.Add("slotSearch.reason.qualificationVerified");
                }
                else
                {
                    externalReviews.Add("slotSearch.external.qualificationNotVerified");
                }

                if (request.PreferredInstructorId == instructor.ExternalResourceId)
                {
                    score += 18;
                    reasons.Add("slotSearch.reason.preferredInstructor");
                }

                if (request.PreferContinuity && instructorContext.HasStudentContinuity)
                {
                    score += 15;
                    reasons.Add("slotSearch.reason.studentContinuity");
                }

                if (vehicle is not null && request.PreferredVehicleId == vehicle.ExternalResourceId)
                {
                    score += 8;
                    reasons.Add("slotSearch.reason.preferredVehicle");
                }

                if (branchId.HasValue)
                    reasons.Add("slotSearch.reason.requestedBranch");

                int instructorLoadDelta = Math.Max(0, instructorLoad - minimumInstructorLoad);
                score -= Math.Min(12, instructorLoadDelta / 120);
                if (instructorLoad == minimumInstructorLoad)
                    reasons.Add("slotSearch.reason.balancedInstructorLoad");

                if (vehicle is not null)
                {
                    int vehicleLoadDelta = Math.Max(0, vehicleLoad - minimumVehicleLoad);
                    score -= Math.Min(6, vehicleLoadDelta / 180);
                    if (vehicleLoad == minimumVehicleLoad)
                        reasons.Add("slotSearch.reason.balancedVehicleUtilization");
                }

                int delayHours = Math.Max(0, (int)(start - normalizedFrom).TotalHours);
                score -= Math.Min(30, delayHours / 24);
                reasons.Add("slotSearch.reason.earliestCompatibleSlot");

                externalReviews.Add("slotSearch.external.costNotEvaluated");
                externalReviews.Add("slotSearch.external.preciseRoutingNotEvaluated");
                foreach (string warning in instructorContext.Warnings)
                    externalReviews.Add(warning);

                suggestions.Add(new SlotSearchSuggestion(
                    start,
                    end,
                    branchId?.Value ?? instructor.BranchId?.Value ?? vehicle?.BranchId?.Value ?? room?.BranchId?.Value,
                    instructor.ExternalResourceId,
                    instructor.Id.Value,
                    instructor.DisplayName,
                    vehicle?.ExternalResourceId,
                    vehicle?.Id.Value,
                    vehicle?.DisplayName,
                    room?.Id.Value,
                    room?.DisplayName,
                    instructorContext.QualificationVerified,
                    instructorContext.HasStudentContinuity,
                    instructorLoad,
                    vehicleLoad,
                    score,
                    score >= 135 ? "excellent" : score >= 115 ? "veryGood" : score >= 100 ? "good" : "compatible",
                    reasons.Distinct().ToArray(),
                    externalReviews.Distinct().ToArray()));

                if (suggestions.Count >= request.MaxSuggestions * 5)
                    break;
            }

            if (suggestions.Count >= request.MaxSuggestions * 5)
                break;
        }

        return BuildResponse(request, evaluated, suggestions, warnings);
    }

    private static SlotSearchResponse BuildResponse(
        SlotSearchRequest request,
        int evaluated,
        IEnumerable<SlotSearchSuggestion> suggestions,
        IEnumerable<string> warnings)
    {
        SlotSearchSuggestion[] ordered = suggestions
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.HasStudentContinuity)
            .ThenBy(x => x.InstructorScheduledMinutes)
            .ThenBy(x => x.StartAtUtc)
            .Take(request.MaxSuggestions)
            .ToArray();

        return new SlotSearchResponse(
            request.FromUtc.ToUniversalTime(),
            request.ToUtc.ToUniversalTime(),
            request.DurationMinutes,
            evaluated,
            ordered,
            warnings.Distinct().ToArray());
    }

    private async Task<List<CalendarResource>> Candidates(
        OrganizationId organizationId,
        BranchId? branchId,
        CalendarResourceType type,
        Guid? preferredExternalId,
        CancellationToken cancellationToken)
    {
        IQueryable<CalendarResource> query = dbContext.CalendarResources.AsNoTracking()
            .Where(x => x.OrganizationId == organizationId
                && x.ResourceType == type
                && x.Status == CalendarResourceStatus.Active);

        if (branchId.HasValue)
            query = query.Where(x => x.BranchId == branchId || x.BranchId == null);

        return await query
            .OrderByDescending(x => preferredExternalId.HasValue && x.ExternalResourceId == preferredExternalId.Value)
            .ThenBy(x => x.DisplayName)
            .Take(MaxResourceCandidatesPerType)
            .ToListAsync(cancellationToken);
    }

    private static Dictionary<Guid, int> BuildScheduledMinutes(
        IEnumerable<Booking> bookings,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc)
    {
        var result = new Dictionary<Guid, int>();
        foreach (Booking booking in bookings)
        {
            DateTimeOffset start = booking.StartAtUtc < fromUtc ? fromUtc : booking.StartAtUtc;
            DateTimeOffset end = booking.EndAtUtc > toUtc ? toUtc : booking.EndAtUtc;
            int minutes = Math.Max(0, (int)Math.Ceiling((end - start).TotalMinutes));
            if (minutes == 0) continue;

            foreach (BookingResource resource in booking.Resources)
                result[resource.CalendarResourceId.Value] = result.GetValueOrDefault(resource.CalendarResourceId.Value) + (minutes * resource.Quantity);
        }
        return result;
    }

    private static SlotSearchResponse Empty(SlotSearchRequest request, IReadOnlyCollection<string> warnings) =>
        new(request.FromUtc.ToUniversalTime(), request.ToUtc.ToUniversalTime(), request.DurationMinutes, 0, [], warnings);

    private static void Validate(SlotSearchRequest request)
    {
        if (request.StudentId == Guid.Empty)
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.studentRequired");
        if (!Enum.IsDefined(typeof(BookingType), request.BookingType))
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.bookingTypeInvalid");
        if (request.DurationMinutes is < 15 or > 480)
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.durationInvalid", new Dictionary<string, object?> { ["min"] = 15, ["max"] = 480 });
        if (request.FromUtc >= request.ToUtc)
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.periodInvalid");
        if ((request.ToUtc - request.FromUtc).TotalDays > 90)
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.periodTooLong", new Dictionary<string, object?> { ["maxDays"] = 90 });
        if (request.StepMinutes is < 5 or > 120)
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.stepInvalid", new Dictionary<string, object?> { ["min"] = 5, ["max"] = 120 });
        if (request.MaxSuggestions is < 1 or > 50)
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.maxSuggestionsInvalid", new Dictionary<string, object?> { ["min"] = 1, ["max"] = 50 });
        if (request.TrainingCategory is { Length: > 80 })
            throw new SlotSearchValidationException("errors.schedulingCapacity.slotSearch.trainingCategoryTooLong", new Dictionary<string, object?> { ["maxLength"] = 80 });
    }
}
