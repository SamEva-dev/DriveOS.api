using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.TrainingDelivery.Application.GroupSessions;

public sealed record ConfirmedGroupBookingSource(
    OrganizationId OrganizationId,
    BookingId BookingId,
    string Program,
    int Capacity,
    UserId TrainerId,
    BranchId? BranchId,
    Guid? RoomResourceId,
    string? RoomName,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    string? SharedObjectives,
    IReadOnlyCollection<PersonId> ParticipantStudentIds);

public interface IConfirmedGroupBookingSourceGateway
{
    Task<Result<ConfirmedGroupBookingSource>> GetAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken ct = default);
}

public sealed record GroupTrainingSessionParticipantResponse(
    Guid Id,
    Guid StudentId,
    bool AddedOutsideOriginalList,
    int AttendanceStatus,
    int? AttendanceMethod,
    DateTimeOffset? CheckInAtUtc,
    DateTimeOffset? CheckOutAtUtc,
    Guid? CompetencyId,
    int? AssessmentLevel,
    decimal? QuizScore,
    string? IndividualObservation,
    int CertificateStatus);

public sealed record GroupTrainingSessionResponse(
    Guid Id,
    Guid SourceBookingId,
    string Program,
    int Capacity,
    Guid TrainerId,
    Guid? BranchId,
    Guid? RoomResourceId,
    string? RoomName,
    DateTimeOffset PlannedStartAtUtc,
    DateTimeOffset PlannedEndAtUtc,
    string? SharedObjectives,
    string? CollectiveReport,
    int RegisteredCount,
    int PresentCount,
    int AbsentCount,
    IReadOnlyCollection<GroupTrainingSessionParticipantResponse> Participants);

public sealed record MaterializeGroupTrainingSessionCommand(OrganizationId OrganizationId, BookingId BookingId) : ICommand<GroupTrainingSessionId>;
public sealed record GetGroupTrainingSessionQuery(OrganizationId OrganizationId, GroupTrainingSessionId SessionId) : IQuery<GroupTrainingSessionResponse>;
public sealed record AddGroupParticipantCommand(OrganizationId OrganizationId, GroupTrainingSessionId SessionId, PersonId StudentId, Guid OperationId) : ICommand<GroupTrainingSessionResponse>;
public sealed record RecordGroupAttendanceCommand(OrganizationId OrganizationId, GroupTrainingSessionId SessionId, PersonId StudentId, GroupSessionAttendanceStatus Status, GroupSessionAttendanceMethod Method, DateTimeOffset? CheckInAtUtc, DateTimeOffset? CheckOutAtUtc, UserId ActorUserId, Guid OperationId) : ICommand<GroupTrainingSessionResponse>;
public sealed record RecordGroupAssessmentCommand(OrganizationId OrganizationId, GroupTrainingSessionId SessionId, PersonId StudentId, Guid? CompetencyId, int? Level, decimal? QuizScore, string? Observation, UserId ActorUserId, Guid OperationId) : ICommand<GroupTrainingSessionResponse>;
public sealed record SaveGroupReportCommand(OrganizationId OrganizationId, GroupTrainingSessionId SessionId, string Report, string? SharedObjectives, Guid OperationId) : ICommand<GroupTrainingSessionResponse>;
public sealed record PrepareGroupCertificateCommand(OrganizationId OrganizationId, GroupTrainingSessionId SessionId, PersonId StudentId, Guid OperationId) : ICommand<GroupTrainingSessionResponse>;
