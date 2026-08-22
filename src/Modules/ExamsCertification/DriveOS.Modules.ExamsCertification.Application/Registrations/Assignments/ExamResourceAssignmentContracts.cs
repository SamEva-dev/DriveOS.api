using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Assignments;

public sealed record ExamInstructorAssignmentEligibility(
    bool IsEligible,
    UserId InstructorId,
    bool QualificationVerified,
    bool AvailabilityVerified,
    IReadOnlyCollection<string> Warnings);

public sealed record ExamVehicleAssignmentEligibility(
    bool IsEligible,
    VehicleId VehicleId,
    bool TechnicalCompatibilityVerified,
    bool InsuranceVerified,
    bool MaintenanceVerified,
    bool LocationVerified,
    bool OwnershipVerified,
    IReadOnlyCollection<string> BlockingReasons,
    IReadOnlyCollection<string> ExternalReviews);

public sealed record ExamVehicleAssignmentRequirements(
    string TrainingCategory,
    string? TransmissionType,
    bool DualControlRequired,
    IReadOnlyCollection<string> RequiredAdaptations,
    string? EnergyType);

public sealed record ExamSchedulingReservationRequest(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    BranchId? BranchId,
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    CalendarResourceId? InstructorCalendarResourceId,
    CalendarResourceId? VehicleCalendarResourceId,
    string TrainingCategory,
    string MeetingPoint,
    Guid OperationId,
    UserId ActorUserId);

public interface IExamResourceAssignmentGateway
{
    Task<Result<ExamInstructorAssignmentEligibility>> EvaluateInstructorAsync(
        OrganizationId organizationId, PersonId studentId, CalendarResourceId calendarResourceId, BranchId? branchId,
        string trainingCategory, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default);

    Task<Result<ExamVehicleAssignmentEligibility>> EvaluateVehicleAsync(
        OrganizationId organizationId, CalendarResourceId calendarResourceId, BranchId? branchId,
        ExamVehicleAssignmentRequirements requirements, DateTimeOffset startAtUtc, DateTimeOffset endAtUtc, CancellationToken cancellationToken = default);

    Task<Result<BookingId>> ReserveSchedulingAsync(ExamSchedulingReservationRequest request, CancellationToken cancellationToken = default);
}

public sealed record AssignExamResourcesCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    CalendarResourceId? InstructorCalendarResourceId,
    CalendarResourceId? VehicleCalendarResourceId,
    string TrainingCategory,
    string? TransmissionType,
    bool DualControlRequired,
    IReadOnlyCollection<string> RequiredAdaptations,
    string? EnergyType,
    Guid OperationId,
    UserId ActorUserId) : ICommand<ExamResourceAssignmentResponse>;

public sealed record GetExamResourceAssignmentQuery(OrganizationId OrganizationId, ExamRegistrationId RegistrationId)
    : IQuery<ExamResourceAssignmentResponse>;

public sealed record ExamResourceAssignmentResponse(
    Guid Id,
    Guid RegistrationId,
    Guid StudentId,
    Guid OperationalPlanId,
    int ConvocationVersion,
    Guid? InstructorCalendarResourceId,
    Guid? InstructorId,
    bool InstructorQualificationVerified,
    bool InstructorAvailabilityVerified,
    IReadOnlyCollection<string> InstructorWarnings,
    Guid? VehicleCalendarResourceId,
    Guid? VehicleId,
    bool VehicleTechnicalCompatibilityVerified,
    bool VehicleInsuranceVerified,
    bool VehicleMaintenanceVerified,
    bool VehicleLocationVerified,
    bool VehicleOwnershipVerified,
    IReadOnlyCollection<string> VehicleExternalReviews,
    Guid? SchedulingBookingId,
    string? SchedulingErrorCode,
    string Status,
    Guid OperationId);
