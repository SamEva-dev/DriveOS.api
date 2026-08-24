using DomainRelay.Abstractions;
using DriveOS.Modules.ExamsCertification.Application.Registrations.Assignments;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Application.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Application.Replacements;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamResourceAssignmentGateway(
    ICalendarResourceReadService resources,
    IBookingReadService bookings,
    IInstructorEligibilityGateway instructorEligibility,
    IInstructorWorkforceAvailabilityGateway workforceAvailability,
    IVehicleReplacementEligibilityGateway vehicleEligibility,
    IMediator mediator) : IExamResourceAssignmentGateway
{
    public async Task<Result<ExamInstructorAssignmentEligibility>> EvaluateInstructorAsync(
        OrganizationId organizationId, PersonId studentId, CalendarResourceId calendarResourceId, BranchId? branchId,
        string trainingCategory, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default)
    {
        CalendarResourceResponse? resource = await resources.GetAsync(organizationId, calendarResourceId, cancellationToken);
        if (resource is null || !string.Equals(resource.ResourceType, CalendarResourceType.Instructor.ToString(), StringComparison.OrdinalIgnoreCase))
            return Result.Failure<ExamInstructorAssignmentEligibility>(ExamResourceAssignmentErrors.InstructorNotEligible);

        bool calendarAvailable = await IsResourceAvailableAsync(organizationId, calendarResourceId.Value, startAtUtc, endAtUtc, cancellationToken);
        var instructorId = new UserId(resource.ExternalResourceId);
        InstructorEligibility eligibility = await instructorEligibility.VerifyAsync(organizationId, instructorId, branchId, trainingCategory, cancellationToken);

        BranchId? effectiveBranch = branchId ?? (resource.BranchId.HasValue ? new BranchId(resource.BranchId.Value) : null);
        InstructorWorkforceAvailabilityResult workforce = await workforceAvailability.CheckAsync(
            organizationId,
            instructorId,
            startAtUtc,
            endAtUtc,
            effectiveBranch,
            resource.TimeZoneId,
            cancellationToken);

        bool available = calendarAvailable && !workforce.IsUnavailable;
        IReadOnlyList<string> warnings = workforce.IsUnavailable && !string.IsNullOrWhiteSpace(workforce.Reason)
            ? eligibility.Warnings.Concat([workforce.Reason]).ToArray()
            : eligibility.Warnings;

        return Result.Success(new ExamInstructorAssignmentEligibility(
            eligibility.IsEligible && available, instructorId, eligibility.IsEligible, available, warnings));
    }

    public async Task<Result<ExamVehicleAssignmentEligibility>> EvaluateVehicleAsync(
        OrganizationId organizationId, CalendarResourceId calendarResourceId, BranchId? branchId,
        ExamVehicleAssignmentRequirements requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default)
    {
        CalendarResourceResponse? resource = await resources.GetAsync(organizationId, calendarResourceId, cancellationToken);
        bool typeOk = resource is not null && (string.Equals(resource.ResourceType, CalendarResourceType.ExamVehicle.ToString(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(resource.ResourceType, CalendarResourceType.Vehicle.ToString(), StringComparison.OrdinalIgnoreCase));
        if (!typeOk) return Result.Failure<ExamVehicleAssignmentEligibility>(ExamResourceAssignmentErrors.VehicleNotEligible);

        bool available = await IsResourceAvailableAsync(organizationId, calendarResourceId.Value, startAtUtc, endAtUtc, cancellationToken);
        var vehicleId = new VehicleId(resource!.ExternalResourceId);
        VehicleReplacementEligibility e = await vehicleEligibility.EvaluateAsync(
            organizationId, vehicleId.Value, branchId,
            new VehicleReplacementRequirements(requirements.TrainingCategory, requirements.TransmissionType, requirements.DualControlRequired,
                requirements.RequiredAdaptations, requirements.EnergyType), startAtUtc, endAtUtc, cancellationToken);
        var blockers = e.BlockingReasons.ToList();
        if (!available) blockers.Add("SchedulingConflict");
        return Result.Success(new ExamVehicleAssignmentEligibility(
            e.IsEligible && available, vehicleId, e.TechnicalCompatibilityVerified, e.InsuranceVerified,
            e.MaintenanceVerified, e.LocationVerified && available, e.OwnershipVerified, blockers, e.ExternalReviews));
    }

    public async Task<Result<BookingId>> ReserveSchedulingAsync(ExamSchedulingReservationRequest request, CancellationToken cancellationToken = default)
    {
        var bookingResources = new List<CreateBookingResourceRequest>();
        if (request.InstructorCalendarResourceId.HasValue) bookingResources.Add(new CreateBookingResourceRequest(request.InstructorCalendarResourceId.Value.Value, 1));
        if (request.VehicleCalendarResourceId.HasValue) bookingResources.Add(new CreateBookingResourceRequest(request.VehicleCalendarResourceId.Value.Value, 1));
        string key = $"exam-resource:{request.OperationId:N}";
        Result<BookingId> created = await mediator.Send(new CreateBookingCommand(
            request.OrganizationId, key, request.BranchId, (int)BookingType.Exam, request.StartAtUtc, request.EndAtUtc,
            "Exam support", request.TrainingPathId.Value, request.TrainingCategory, null, request.MeetingPoint,
            null, null, null, null, (int)BookingNotificationPolicy.OnConfirmation, bookingResources,
            [new CreateBookingParticipantRequest((int)BookingParticipantType.Student, request.StudentId.Value)]), cancellationToken);
        if (created.IsFailure) return created;

        Result<BookingConflictCheckResponse> reserved = await mediator.Send(new ReserveBookingCommand(request.OrganizationId, created.Value), cancellationToken);
        if (reserved.IsFailure) return Result.Failure<BookingId>(reserved.Error);
        if (!reserved.Value.IsConflictFree) return Result.Failure<BookingId>(ExamResourceAssignmentErrors.SchedulingFailed);

        Result<BookingConflictCheckResponse> confirmed = await mediator.Send(new ConfirmBookingCommand(request.OrganizationId, created.Value, request.ActorUserId), cancellationToken);
        if (confirmed.IsFailure) return Result.Failure<BookingId>(confirmed.Error);
        if (!confirmed.Value.IsConflictFree) return Result.Failure<BookingId>(ExamResourceAssignmentErrors.SchedulingFailed);
        return Result.Success(created.Value);
    }

    private async Task<bool> IsResourceAvailableAsync(OrganizationId organizationId, Guid calendarResourceId, DateTimeOffset start, DateTimeOffset end, CancellationToken ct)
    {
        IReadOnlyCollection<BookingResponse> existing = await bookings.ListAsync(organizationId, null, start, end, ct);
        return !existing.Any(b => b.Resources.Any(r => r.CalendarResourceId == calendarResourceId)
            && b.StartAtUtc < end && b.EndAtUtc > start
            && b.Status is (int)BookingStatus.Reserved or (int)BookingStatus.Confirmed or (int)BookingStatus.Tentative);
    }
}
