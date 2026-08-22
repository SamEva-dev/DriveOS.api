using System.Security.Cryptography;
using System.Text;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Assignments;

public sealed class AssignExamResourcesCommandHandler(
    IExamRegistrationRepository registrations,
    IExamOperationalPlanRepository plans,
    IExamResourceAssignmentRepository assignments,
    IExamResourceAssignmentGateway gateway,
    IExamsCertificationUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AssignExamResourcesCommand, ExamResourceAssignmentResponse>
{
    public async Task<Result<ExamResourceAssignmentResponse>> Handle(AssignExamResourcesCommand command, CancellationToken cancellationToken)
    {
        if (command.OperationId == Guid.Empty)
            return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.InvalidIdentifier);

        string fingerprint = Fingerprint(command);
        ExamResourceAssignment? replay = await assignments.GetByOperationIdAsync(command.OrganizationId, command.OperationId, cancellationToken);
        if (replay is not null)
            return string.Equals(replay.RequestFingerprint, fingerprint, StringComparison.Ordinal)
                ? Result.Success(Map(replay))
                : Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.OperationConflict);

        ExamRegistration? registration = await registrations.GetByIdAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (registration is null) return Result.Failure<ExamResourceAssignmentResponse>(ExamRegistrationErrors.NotFound);

        ExamOperationalPlan? plan = await plans.GetByRegistrationAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (plan is null) return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.OperationalPlanRequired);
        if (plan.Status != ExamOperationalPlanStatus.ReadyForAssignment)
            return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.OperationalPlanNotReady);

        ExamInstructorAssignmentEligibility? instructor = null;
        if (plan.InstructorRequired)
        {
            if (!command.InstructorCalendarResourceId.HasValue)
                return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.InstructorRequired);
            Result<ExamInstructorAssignmentEligibility> evaluated = await gateway.EvaluateInstructorAsync(
                command.OrganizationId, registration.StudentId, command.InstructorCalendarResourceId.Value, plan.DepartureBranchId,
                command.TrainingCategory, plan.OperationalWindowStartUtc, plan.OperationalWindowEndUtc, cancellationToken);
            if (evaluated.IsFailure) return Result.Failure<ExamResourceAssignmentResponse>(evaluated.Error);
            instructor = evaluated.Value;
            if (!instructor.IsEligible) return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.InstructorNotEligible);
        }

        ExamVehicleAssignmentEligibility? vehicle = null;
        if (plan.VehicleRequired)
        {
            if (!command.VehicleCalendarResourceId.HasValue)
                return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.VehicleRequired);
            Result<ExamVehicleAssignmentEligibility> evaluated = await gateway.EvaluateVehicleAsync(
                command.OrganizationId, command.VehicleCalendarResourceId.Value, plan.DepartureBranchId,
                new ExamVehicleAssignmentRequirements(command.TrainingCategory, command.TransmissionType, command.DualControlRequired,
                    command.RequiredAdaptations ?? [], command.EnergyType),
                plan.OperationalWindowStartUtc, plan.OperationalWindowEndUtc, cancellationToken);
            if (evaluated.IsFailure) return Result.Failure<ExamResourceAssignmentResponse>(evaluated.Error);
            vehicle = evaluated.Value;
            if (!vehicle.IsEligible) return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.VehicleNotEligible);
        }

        ExamResourceAssignment? current = await assignments.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (current is not null && current.Status == ExamResourceAssignmentStatus.Assigned)
            return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.OperationConflict);

        Result<ExamResourceAssignment> created = ExamResourceAssignment.Create(
            ExamResourceAssignmentId.New(), command.OrganizationId, command.RegistrationId, registration.StudentId, plan.Id, plan.ConvocationVersion,
            command.OperationId, fingerprint,
            command.InstructorCalendarResourceId, instructor?.InstructorId, plan.InstructorRequired,
            instructor?.QualificationVerified ?? !plan.InstructorRequired, instructor?.AvailabilityVerified ?? !plan.InstructorRequired, instructor?.Warnings,
            command.VehicleCalendarResourceId, vehicle?.VehicleId, plan.VehicleRequired,
            vehicle?.TechnicalCompatibilityVerified ?? !plan.VehicleRequired, vehicle?.InsuranceVerified ?? !plan.VehicleRequired,
            vehicle?.MaintenanceVerified ?? !plan.VehicleRequired, vehicle?.LocationVerified ?? !plan.VehicleRequired,
            vehicle?.OwnershipVerified ?? !plan.VehicleRequired, vehicle?.ExternalReviews,
            command.ActorUserId, clock.UtcNow);
        if (created.IsFailure) return Result.Failure<ExamResourceAssignmentResponse>(created.Error);

        ExamResourceAssignment assignment = created.Value;
        assignments.Add(assignment);
        await unitOfWork.CommitAsync(cancellationToken);

        Result<BookingId> scheduling = await gateway.ReserveSchedulingAsync(new ExamSchedulingReservationRequest(
            command.OrganizationId, command.RegistrationId, registration.StudentId, registration.TrainingPathId,
            plan.DepartureBranchId, plan.OperationalWindowStartUtc, plan.OperationalWindowEndUtc,
            command.InstructorCalendarResourceId, command.VehicleCalendarResourceId, command.TrainingCategory,
            plan.MeetingInstructions ?? string.Empty, command.OperationId, command.ActorUserId), cancellationToken);

        ExamResourceAssignment? tracked = await assignments.GetByRegistrationForUpdateAsync(command.OrganizationId, command.RegistrationId, cancellationToken);
        if (tracked is null) return Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.NotFound);
        if (scheduling.IsFailure)
        {
            tracked.MarkSchedulingFailed(scheduling.Error.Code, command.ActorUserId, clock.UtcNow);
            await unitOfWork.CommitAsync(cancellationToken);
            return Result.Failure<ExamResourceAssignmentResponse>(scheduling.Error);
        }

        Result marked = tracked.MarkScheduled(scheduling.Value, command.ActorUserId, clock.UtcNow);
        if (marked.IsFailure) return Result.Failure<ExamResourceAssignmentResponse>(marked.Error);
        await unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(Map(tracked));
    }

    private static string Fingerprint(AssignExamResourcesCommand c)
    {
        string canonical = string.Join('|', c.RegistrationId.Value.ToString("N"), c.InstructorCalendarResourceId?.Value.ToString("N") ?? "",
            c.VehicleCalendarResourceId?.Value.ToString("N") ?? "", c.TrainingCategory?.Trim() ?? "", c.TransmissionType?.Trim() ?? "",
            c.DualControlRequired, string.Join(',', (c.RequiredAdaptations ?? []).OrderBy(x => x, StringComparer.Ordinal)), c.EnergyType?.Trim() ?? "");
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    internal static ExamResourceAssignmentResponse Map(ExamResourceAssignment x) => new(
        x.Id.Value, x.RegistrationId.Value, x.StudentId.Value, x.OperationalPlanId.Value, x.ConvocationVersion,
        x.InstructorCalendarResourceId?.Value, x.InstructorId?.Value, x.InstructorQualificationVerified, x.InstructorAvailabilityVerified, x.InstructorWarnings,
        x.VehicleCalendarResourceId?.Value, x.VehicleId?.Value, x.VehicleTechnicalCompatibilityVerified, x.VehicleInsuranceVerified,
        x.VehicleMaintenanceVerified, x.VehicleLocationVerified, x.VehicleOwnershipVerified, x.VehicleExternalReviews,
        x.SchedulingBookingId?.Value, x.SchedulingErrorCode, x.Status.ToString(), x.OperationId);
}

public sealed class GetExamResourceAssignmentQueryHandler(IExamResourceAssignmentRepository assignments)
    : IQueryHandler<GetExamResourceAssignmentQuery, ExamResourceAssignmentResponse>
{
    public async Task<Result<ExamResourceAssignmentResponse>> Handle(GetExamResourceAssignmentQuery query, CancellationToken cancellationToken)
    {
        ExamResourceAssignment? assignment = await assignments.GetByRegistrationAsync(query.OrganizationId, query.RegistrationId, cancellationToken);
        return assignment is null
            ? Result.Failure<ExamResourceAssignmentResponse>(ExamResourceAssignmentErrors.NotFound)
            : Result.Success(AssignExamResourcesCommandHandler.Map(assignment));
    }
}
