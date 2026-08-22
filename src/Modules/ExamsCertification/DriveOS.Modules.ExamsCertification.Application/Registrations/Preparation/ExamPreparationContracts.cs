using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Preparation;

public sealed record ExamPreparationSourceSnapshot(
    int ConvocationVersion,
    bool InstructorRequired,
    bool VehicleRequired,
    IReadOnlyCollection<ExamPreparationCheckSnapshot> Checks);

public interface IExamPreparationSnapshotGateway
{
    Task<Result<ExamPreparationSourceSnapshot>> BuildAsync(
        OrganizationId organizationId,
        ExamRegistrationId registrationId,
        CancellationToken cancellationToken = default);
}

public sealed record RefreshExamPreparationCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    bool MeetingPointConfirmed,
    bool VehicleEnergyConfirmed,
    bool InstructorConfirmed,
    bool InstructionsTransmitted,
    IReadOnlyCollection<int>? ReminderOffsetsDays,
    Guid OperationId,
    UserId ActorUserId) : ICommand<ExamPreparationResponse>;

public sealed record ConfirmExamPreparationCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    UserId ActorUserId) : ICommand<ExamPreparationResponse>;

public sealed record GetExamPreparationQuery(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : IQuery<ExamPreparationResponse>;

public sealed record ExamPreparationCheckResponse(
    string Code,
    bool Required,
    string Status,
    string MessageKey,
    string Source,
    string? Evidence);

public sealed record ExamPreparationResponse(
    Guid Id,
    Guid RegistrationId,
    Guid StudentId,
    int Revision,
    int ConvocationVersion,
    string Status,
    bool IsReady,
    bool IsConfirmed,
    int? ConfirmedRevision,
    DateTimeOffset? ConfirmedAtUtc,
    Guid? ConfirmedByUserId,
    bool MeetingPointConfirmed,
    bool VehicleEnergyConfirmed,
    bool InstructorConfirmed,
    bool InstructionsTransmitted,
    IReadOnlyCollection<int> ReminderOffsetsDays,
    DateTimeOffset? LastEvaluatedAtUtc,
    IReadOnlyCollection<ExamPreparationCheckResponse> Checks);
