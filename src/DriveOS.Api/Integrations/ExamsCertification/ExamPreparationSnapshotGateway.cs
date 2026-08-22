using DriveOS.Modules.ExamsCertification.Application.Registrations.Preparation;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.Modules.FleetResources.Application.Vehicles;
using DriveOS.Modules.SchedulingCapacity.Application.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Integrations.ExamsCertification;

internal sealed class ExamPreparationSnapshotGateway(
    IExamRegistrationRepository registrations,
    IExamReadinessDecisionRepository readinessDecisions,
    IExamConvocationRepository convocations,
    IExamRegistrationFileRepository registrationFiles,
    IExamOperationalPlanRepository operationalPlans,
    IExamResourceAssignmentRepository assignments,
    IFleetVehicleComplianceReadService fleetCompliance,
    IBookingReadService bookings) : IExamPreparationSnapshotGateway
{
    public async Task<Result<ExamPreparationSourceSnapshot>> BuildAsync(
        OrganizationId organizationId, ExamRegistrationId registrationId, CancellationToken cancellationToken = default)
    {
        ExamRegistration? registration = await registrations.GetByIdAsync(organizationId, registrationId, cancellationToken);
        if (registration is null) return Result.Failure<ExamPreparationSourceSnapshot>(ExamPreparationErrors.NotFound);

        ExamConvocation? convocation = await convocations.GetByRegistrationAsync(organizationId, registrationId, cancellationToken);
        if (convocation?.CurrentRevision is null)
            return Result.Failure<ExamPreparationSourceSnapshot>(ExamPreparationErrors.ConvocationMissing);

        ExamRegistrationFile? file = await registrationFiles.GetByRegistrationAsync(organizationId, registrationId, cancellationToken);
        ExamOperationalPlan? plan = await operationalPlans.GetByRegistrationAsync(organizationId, registrationId, cancellationToken);
        ExamResourceAssignment? assignment = await assignments.GetByRegistrationAsync(organizationId, registrationId, cancellationToken);

        var checks = new List<ExamPreparationCheckSnapshot>();

        ExamReadinessDecision? readiness = await readinessDecisions.GetCurrentAsync(
            organizationId, registration.StudentId, registration.TrainingPathId, cancellationToken);

        ExamPreparationCheckStatus readinessStatus = readiness?.Outcome switch
        {
            ExamReadinessOutcome.Ready => ExamPreparationCheckStatus.Ready,
            ExamReadinessOutcome.ReadyWithConditions => ExamPreparationCheckStatus.Pending,
            null => ExamPreparationCheckStatus.Blocked,
            _ => ExamPreparationCheckStatus.Blocked
        };

        checks.Add(Check(
            "ReadinessDecisionCurrent",
            true,
            readinessStatus,
            "exams.preparation.readinessDecisionCurrent",
            "ExamsCertification",
            readiness is null
                ? "readiness-decision-missing"
                : $"decisionId={readiness.Id.Value};version={readiness.Version};outcome={readiness.Outcome};decidedAt={readiness.DecidedAtUtc:O}"));

        checks.Add(Check("ConvocationConfirmed", true,
            ExamPreparationCheckStatus.Ready,
            "exams.preparation.convocationConfirmed", "ExamsCertification",
            $"version={convocation.CurrentVersion};start={convocation.CurrentRevision.ScheduledStartUtc:O}"));

        bool documentsReady = file?.CurrentRevision is not null
            && file.Status == ExamRegistrationFileStatus.OfficiallyAccepted
            && file.CurrentRevision.Checklist.Where(x => x.Required).All(x => x.Status is ExamRegistrationRequirementStatus.Compliant or ExamRegistrationRequirementStatus.NotApplicable);
        checks.Add(Check("DocumentsAvailable", true,
            documentsReady ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Blocked,
            "exams.preparation.documentsAvailable", "ExamsCertification",
            file is null ? "registration-file-missing" : $"fileVersion={file.CurrentVersion};status={file.Status}"));

        bool requiredDocumentsListed = !string.IsNullOrWhiteSpace(convocation.CurrentRevision.RequiredDocuments);
        checks.Add(Check("RequiredDocumentsListed", true,
            requiredDocumentsListed ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Warning,
            "exams.preparation.requiredDocumentsListed", "ExamsCertification",
            convocation.CurrentRevision.RequiredDocuments));

        bool studentInformed = convocation.DeliveryStatus is ExamConvocationDeliveryStatus.Delivered or ExamConvocationDeliveryStatus.Acknowledged;
        checks.Add(Check("StudentInformed", true,
            studentInformed ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Pending,
            "exams.preparation.studentInformed", "CommunicationEngagement",
            $"deliveryStatus={convocation.DeliveryStatus}"));

        bool planCurrent = plan is not null && plan.ConvocationVersion == convocation.CurrentVersion
            && plan.MeetingAtUtc < convocation.CurrentRevision.ScheduledStartUtc;
        checks.Add(Check("OperationalPlanCurrent", true,
            planCurrent ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Blocked,
            "exams.preparation.operationalPlanCurrent", "ExamsCertification",
            plan is null ? "plan-missing" : $"planConvocationVersion={plan.ConvocationVersion};officialVersion={convocation.CurrentVersion}"));

        bool assignmentCurrent = assignment is not null && assignment.Status == ExamResourceAssignmentStatus.Assigned
            && assignment.ConvocationVersion == convocation.CurrentVersion;
        checks.Add(Check("ResourcesAssigned", true,
            assignmentCurrent ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Blocked,
            "exams.preparation.resourcesAssigned", "ExamsCertification",
            assignment is null ? "assignment-missing" : $"assignmentStatus={assignment.Status};convocationVersion={assignment.ConvocationVersion}"));

        bool vehicleRequired = plan?.VehicleRequired ?? true;
        bool instructorRequired = plan?.InstructorRequired ?? true;

        bool instructorReady = !instructorRequired
            || (assignmentCurrent
                && assignment?.InstructorId is not null
                && assignment.InstructorQualificationVerified
                && assignment.InstructorAvailabilityVerified);

        checks.Add(Check(
            "InstructorVerified",
            instructorRequired,
            !instructorRequired
                ? ExamPreparationCheckStatus.NotApplicable
                : instructorReady
                    ? ExamPreparationCheckStatus.Ready
                    : ExamPreparationCheckStatus.Blocked,
            "exams.preparation.instructorVerified",
            "ExamsCertification",
            !instructorRequired
                ? "instructor-not-required"
                : assignment?.InstructorId is { } instructorId
                    ? $"instructorId={instructorId.Value};qualification={assignment.InstructorQualificationVerified};availability={assignment.InstructorAvailabilityVerified}"
                    : "instructor-assignment-missing"));
        if (!vehicleRequired)
        {
            checks.Add(Check("VehicleVerified", false, ExamPreparationCheckStatus.NotApplicable,
                "exams.preparation.vehicleVerified", "FleetResources", "vehicle-not-required"));
        }
        else if (assignment?.VehicleId is { } vehicleId && plan is not null)
        {
            FleetVehicleComplianceEvaluation fleet = await fleetCompliance.EvaluateAsync(
                organizationId, vehicleId, plan.DepartureBranchId,
                new FleetVehicleComplianceRequirement(registration.LicenseCategory, null, false, [], null),
                plan.OperationalWindowStartUtc, plan.OperationalWindowEndUtc, cancellationToken);
            bool vehicleReady = assignment.VehicleTechnicalCompatibilityVerified
                && assignment.VehicleInsuranceVerified && assignment.VehicleMaintenanceVerified
                && assignment.VehicleLocationVerified && assignment.VehicleOwnershipVerified
                && fleet.IsEligible;
            checks.Add(Check("VehicleVerified", true,
                vehicleReady ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Blocked,
                "exams.preparation.vehicleVerified", "FleetResources",
                vehicleReady ? $"vehicleId={vehicleId.Value}" : string.Join(',', fleet.BlockingReasons)));
        }
        else
        {
            checks.Add(Check("VehicleVerified", true, ExamPreparationCheckStatus.Blocked,
                "exams.preparation.vehicleVerified", "FleetResources", "vehicle-assignment-missing"));
        }

        bool bookingReady = assignment?.SchedulingBookingId is { } bookingId
            && (await bookings.GetAsync(organizationId, bookingId, cancellationToken)) is { Status: (int)BookingStatus.Confirmed };
        checks.Add(Check("ExamBookingConfirmed", true,
            bookingReady ? ExamPreparationCheckStatus.Ready : ExamPreparationCheckStatus.Blocked,
            "exams.preparation.examBookingConfirmed", "SchedulingCapacity",
            assignment?.SchedulingBookingId is { } b ? $"bookingId={b.Value}" : "booking-missing"));

        DateTimeOffset from = convocation.CurrentRevision.ScheduledStartUtc.AddDays(-30);
        IReadOnlyCollection<BookingResponse> recent = await bookings.ListAsync(organizationId, null, from, convocation.CurrentRevision.ScheduledStartUtc, cancellationToken);
        BookingResponse? lastLesson = recent
            .Where(b => b.BookingType == (int)BookingType.TrainingSession
                && b.Participants.Any(p => p.ParticipantType == (int)BookingParticipantType.Student && p.ExternalParticipantId == convocation.StudentId.Value)
                && b.Status is (int)BookingStatus.Reserved or (int)BookingStatus.Confirmed)
            .OrderByDescending(b => b.StartAtUtc)
            .FirstOrDefault();
        checks.Add(Check("LastLessonPlanned", false,
            lastLesson is null ? ExamPreparationCheckStatus.Warning : ExamPreparationCheckStatus.Ready,
            "exams.preparation.lastLessonPlanned", "SchedulingCapacity",
            lastLesson is null ? null : $"bookingId={lastLesson.Id};start={lastLesson.StartAtUtc:O}"));

        return Result.Success(new ExamPreparationSourceSnapshot(convocation.CurrentVersion, instructorRequired, vehicleRequired, checks));
    }

    private static ExamPreparationCheckSnapshot Check(string code, bool required, ExamPreparationCheckStatus status,
        string messageKey, string source, string? evidence = null) =>
        new(code, required, status, messageKey, source, evidence);
}
