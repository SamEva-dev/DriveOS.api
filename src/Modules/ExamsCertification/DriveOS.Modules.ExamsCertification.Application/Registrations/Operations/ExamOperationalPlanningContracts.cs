using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Operations;

public sealed record ExamOperationalResourceCandidate(Guid CalendarResourceId, Guid ExternalResourceId, string DisplayName, Guid? BranchId, bool IsAvailable, IReadOnlyList<string> Conflicts);

public sealed record ExamOperationalPlanningAssessment(
    IReadOnlyList<ExamOperationalResourceCandidate> InstructorCandidates,
    IReadOnlyList<ExamOperationalResourceCandidate> VehicleCandidates,
    IReadOnlyList<string> GeneralConflicts);

public interface IExamOperationalPlanningGateway
{
    Task<ExamOperationalPlanningAssessment> AssessAsync(OrganizationId organizationId, BranchId? departureBranchId,
        DateTimeOffset windowStartUtc, DateTimeOffset windowEndUtc, CancellationToken cancellationToken = default);
}

public sealed record RefreshExamOperationalPlanCommand(
    OrganizationId OrganizationId, ExamRegistrationId RegistrationId, DateTimeOffset? MeetingAtUtc,
    int TravelBufferBeforeMinutes, int TravelBufferAfterMinutes, BranchId? DepartureBranchId,
    bool InstructorRequired, bool VehicleRequired, string? MeetingInstructions, UserId ActorUserId) : ICommand<ExamOperationalPlanResponse>;

public sealed record GetExamOperationalPlanQuery(OrganizationId OrganizationId, ExamRegistrationId RegistrationId) : IQuery<ExamOperationalPlanResponse>;
public sealed record GetExamOperationalPlanningOptionsQuery(OrganizationId OrganizationId, ExamRegistrationId RegistrationId, BranchId? DepartureBranchId,
    DateTimeOffset? MeetingAtUtc, int TravelBufferBeforeMinutes, int TravelBufferAfterMinutes) : IQuery<ExamOperationalPlanningOptionsResponse>;

public sealed record ExamOperationalPlanResponse(Guid Id, Guid RegistrationId, Guid StudentId, int ConvocationVersion,
    DateTimeOffset OfficialStartUtc, DateTimeOffset OfficialEndUtc, DateTimeOffset MeetingAtUtc,
    DateTimeOffset OperationalWindowStartUtc, DateTimeOffset OperationalWindowEndUtc, int TravelBufferBeforeMinutes,
    int TravelBufferAfterMinutes, Guid? DepartureBranchId, bool InstructorRequired, bool VehicleRequired, string? MeetingInstructions,
    bool HasSchedulingConflicts, int InstructorCandidatesAvailable, int VehicleCandidatesAvailable, string? ConflictSummary,
    string Status, DateTimeOffset LastAssessedAtUtc);

public sealed record ExamOperationalPlanningOptionsResponse(DateTimeOffset WindowStartUtc, DateTimeOffset WindowEndUtc,
    IReadOnlyList<ExamOperationalResourceCandidate> Instructors, IReadOnlyList<ExamOperationalResourceCandidate> Vehicles, IReadOnlyList<string> Conflicts);
