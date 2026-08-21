using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.Students.Application.Statuses;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.TrainingDelivery;

internal sealed class TrainingSessionExecutionReadinessGateway(
    IConfirmedBookingSessionSourceGateway sourceGateway,
    IBookingExecutionReadinessService schedulingReadiness,
    ITrainingSessionVehicleComplianceGateway vehicleComplianceGateway,
    IStudentIdentityService studentIdentityService,
    IStudentStatusService studentStatusService)
    : ITrainingSessionExecutionReadinessGateway
{
    public async Task<Result<TrainingSessionExecutionReadiness>> CheckAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default)
    {
        Result<ConfirmedBookingSessionSource> sourceResult = await sourceGateway.GetAsync(organizationId, bookingId, cancellationToken);
        if (sourceResult.IsFailure)
            return Result.Failure<TrainingSessionExecutionReadiness>(sourceResult.Error);

        BookingExecutionReadinessResponse scheduling = await schedulingReadiness.CheckAsync(organizationId, bookingId, cancellationToken);
        if (!scheduling.Exists)
            return Result.Failure<TrainingSessionExecutionReadiness>(TrainingSessionErrors.SourceBookingNotFound);
        if (!scheduling.IsConfirmed)
            return Result.Failure<TrainingSessionExecutionReadiness>(TrainingSessionErrors.SourceBookingNotConfirmed);

        ConfirmedBookingSessionSource source = sourceResult.Value;
        var checks = new List<TrainingSessionReadinessCheck>
        {
            new("source-booking-confirmed", "trainingDelivery.readiness.sourceBookingConfirmed", TrainingSessionReadinessCheckStatus.Passed),
            new("student-linked", "trainingDelivery.readiness.studentLinked", TrainingSessionReadinessCheckStatus.Passed),
            new("instructor-linked", "trainingDelivery.readiness.instructorLinked", TrainingSessionReadinessCheckStatus.Passed)
        };

        StudentIdentityResponse? identity = await studentIdentityService.GetAsync(organizationId, source.StudentId, cancellationToken);
        bool identityVerified = identity?.VerificationStatus is IdentityVerificationStatus.DocumentVerified or IdentityVerificationStatus.ExternallyVerified;
        checks.Add(new TrainingSessionReadinessCheck(
            "student-identity-verified",
            identityVerified ? "trainingDelivery.readiness.studentIdentityVerified" : "trainingDelivery.readiness.studentIdentityVerificationRequired",
            identityVerified ? TrainingSessionReadinessCheckStatus.Passed : TrainingSessionReadinessCheckStatus.Blocked));

        StudentStatusesResponse? studentStatuses = await studentStatusService.GetAsync(organizationId, source.StudentId, cancellationToken);
        bool enrollmentActive = studentStatuses?.EnrollmentStatus == EnrollmentStatus.Active;
        checks.Add(new TrainingSessionReadinessCheck(
            "student-enrollment-active",
            enrollmentActive ? "trainingDelivery.readiness.studentEnrollmentActive" : "trainingDelivery.readiness.studentEnrollmentInactive",
            enrollmentActive ? TrainingSessionReadinessCheckStatus.Passed : TrainingSessionReadinessCheckStatus.Blocked));

        bool lessonAllowed = studentStatuses is not null
            && studentStatuses.SchedulingStatus != SchedulingStatus.Suspended
            && !studentStatuses.CurrentlyBlockedActions.HasFlag(StudentBlockingAction.StartLesson);
        checks.Add(new TrainingSessionReadinessCheck(
            "student-start-lesson-allowed",
            lessonAllowed ? "trainingDelivery.readiness.studentStartLessonAllowed" : "trainingDelivery.readiness.studentStartLessonBlocked",
            lessonAllowed ? TrainingSessionReadinessCheckStatus.Passed : TrainingSessionReadinessCheckStatus.Blocked));

        if (scheduling.IsConflictFree)
        {
            checks.Add(new("scheduling-conflict-free", "trainingDelivery.readiness.schedulingConflictFree", TrainingSessionReadinessCheckStatus.Passed));
        }
        else
        {
            foreach (BookingConflictResponse conflict in scheduling.Conflicts)
            {
                checks.Add(new(
                    $"scheduling-conflict-{conflict.Type}",
                    "trainingDelivery.readiness.schedulingConflict",
                    TrainingSessionReadinessCheckStatus.Blocked,
                    conflict.Reason));
            }
        }

        bool vehicleReady = true;
        if (source.VehicleId.HasValue)
        {
            TrainingSessionVehicleCompliance compliance = await vehicleComplianceGateway.CheckAsync(
                organizationId,
                source.VehicleId.Value,
                source.BranchId,
                source.TrainingCategory,
                source.PlannedStartAtUtc,
                source.PlannedEndAtUtc,
                cancellationToken);

            vehicleReady = compliance.IsVerified && compliance.IsOperational;
            checks.Add(new TrainingSessionReadinessCheck(
                "fleet-authoritative-compliance",
                vehicleReady ? "trainingDelivery.readiness.fleetComplianceVerified" : "trainingDelivery.readiness.fleetComplianceRequired",
                vehicleReady ? TrainingSessionReadinessCheckStatus.Passed : TrainingSessionReadinessCheckStatus.Blocked,
                vehicleReady ? null : string.Join(", ", compliance.BlockingReasons.Concat(compliance.ExternalReviews))));
        }
        else
        {
            checks.Add(new TrainingSessionReadinessCheck(
                "fleet-authoritative-compliance",
                "trainingDelivery.readiness.vehicleNotApplicable",
                TrainingSessionReadinessCheckStatus.NotApplicable));
        }

        return Result.Success(new TrainingSessionExecutionReadiness(
            scheduling.IsReady && vehicleReady && identityVerified && enrollmentActive && lessonAllowed,
            source.BookingId,
            source.StudentId,
            source.TrainingPathId,
            source.InstructorId,
            source.BranchId,
            source.VehicleId,
            source.PlannedStartAtUtc,
            source.PlannedEndAtUtc,
            checks));
    }
}
