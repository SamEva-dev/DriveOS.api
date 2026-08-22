using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.ExamsCertification.Application.Registrations.Convocations;

public sealed record ReceiveExamConvocationCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    ExamCenterId ExamCenterId,
    DateTimeOffset ScheduledStartUtc,
    DateTimeOffset ScheduledEndUtc,
    string ProviderCode,
    string? OfficialReference,
    string? CandidateReference,
    string? Instructions,
    string? RequiredDocuments,
    string? ProviderPayloadReference,
    Guid OperationId,
    UserId ActorUserId) : ICommand<ExamConvocationResponse>;

public sealed record SetExamConvocationMeetingCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    DateTimeOffset? MeetingAtUtc,
    string? Instructions,
    UserId ActorUserId) : ICommand<ExamConvocationResponse>;

public sealed record MarkExamConvocationDeliveredCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    ExamConvocationDeliveryChannel Channel,
    UserId ActorUserId) : ICommand<ExamConvocationResponse>;

public sealed record MarkExamConvocationAcknowledgedCommand(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId,
    UserId ActorUserId) : ICommand<ExamConvocationResponse>;

public sealed record GetExamConvocationQuery(
    OrganizationId OrganizationId,
    ExamRegistrationId RegistrationId) : IQuery<ExamConvocationResponse>;

public sealed record ExamConvocationRevisionResponse(
    Guid Id,
    int Version,
    Guid ExamCenterId,
    string CenterName,
    string? CenterAddress,
    string TimeZoneId,
    DateTimeOffset ScheduledStartUtc,
    DateTimeOffset ScheduledEndUtc,
    string ProviderCode,
    string? OfficialReference,
    string? CandidateReference,
    string? Instructions,
    string? RequiredDocuments,
    string? ProviderPayloadReference,
    DateTimeOffset ReceivedAtUtc);

public sealed record ExamConvocationResponse(
    Guid Id,
    Guid RegistrationId,
    Guid StudentId,
    int CurrentVersion,
    string DeliveryStatus,
    string? DeliveryChannel,
    DateTimeOffset? DeliveredAtUtc,
    DateTimeOffset? AcknowledgedAtUtc,
    DateTimeOffset? InternalMeetingAtUtc,
    string? InternalMeetingInstructions,
    IReadOnlyList<ExamConvocationRevisionResponse> Revisions,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);
