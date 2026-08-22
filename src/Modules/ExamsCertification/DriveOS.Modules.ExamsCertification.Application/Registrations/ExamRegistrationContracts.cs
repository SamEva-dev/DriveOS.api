using DriveOS.Application.Abstractions.Messaging;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations;

public sealed record HoldExamPlaceCommand(
    OrganizationId OrganizationId,
    ExamPlaceId ExamPlaceId,
    int HoldMinutes,
    UserId ActorUserId) : ICommand<ExamPlaceHoldResponse>;

public sealed record ReleaseExamPlaceHoldCommand(
    OrganizationId OrganizationId,
    ExamPlaceId ExamPlaceId,
    Guid HoldToken,
    UserId ActorUserId) : ICommand;

public sealed record CreateExamRegistrationCommand(
    OrganizationId OrganizationId,
    PersonId StudentId,
    TrainingPathId TrainingPathId,
    ExamPlaceId ExamPlaceId,
    Guid HoldToken,
    Guid OperationId,
    UserId ActorUserId) : ICommand<ExamRegistrationResponse>;

public sealed record GetExamRegistrationQuery(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : IQuery<ExamRegistrationResponse>;

public sealed record GetStudentExamRegistrationsQuery(
    OrganizationId OrganizationId,
    PersonId StudentId) : IQuery<IReadOnlyList<ExamRegistrationResponse>>;

public sealed record ExamPlaceHoldResponse(Guid ExamPlaceId, Guid HoldToken, DateTimeOffset ExpiresAtUtc);

public sealed record ExamRegistrationResponse(
    Guid Id,
    Guid StudentId,
    Guid TrainingPathId,
    Guid ReadinessDecisionId,
    Guid ExamPlaceId,
    Guid ExamCenterId,
    string ExamType,
    string LicenseCategory,
    DateTimeOffset ScheduledStartUtc,
    DateTimeOffset ScheduledEndUtc,
    string ProviderCode,
    string? ExternalPlaceId,
    string? ExternalRegistrationId,
    string? CandidateReference,
    string Status,
    DateTimeOffset CreatedAtUtc);
