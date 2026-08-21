using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Incidents;

public sealed record ReportTrainingIncidentCommand(OrganizationId OrganizationId, TrainingSessionId SessionId, Guid OperationId, TrainingIncidentType IncidentType, TrainingIncidentSeverity Severity, DateTimeOffset OccurredAtUtc, string Description, string ImmediateActions, IReadOnlyCollection<TrainingIncidentParticipantInput> AdditionalParticipants, UserId ActorUserId) : ICommand<TrainingIncidentResponse>;
public sealed record AddTrainingIncidentEvidenceCommand(OrganizationId OrganizationId, TrainingIncidentId IncidentId, Guid OperationId, Guid DocumentId, string EvidenceType, string? Description, UserId ActorUserId) : ICommand<TrainingIncidentResponse>;
public sealed record EscalateTrainingIncidentCommand(OrganizationId OrganizationId, TrainingIncidentId IncidentId, Guid OperationId, string Reason, UserId ActorUserId) : ICommand<TrainingIncidentResponse>;
public sealed record StartTrainingIncidentReviewCommand(OrganizationId OrganizationId, TrainingIncidentId IncidentId, Guid OperationId, string? Reason, UserId ActorUserId) : ICommand<TrainingIncidentResponse>;
public sealed record ResolveTrainingIncidentCommand(OrganizationId OrganizationId, TrainingIncidentId IncidentId, Guid OperationId, string Resolution, UserId ActorUserId) : ICommand<TrainingIncidentResponse>;
public sealed record CloseTrainingIncidentCommand(OrganizationId OrganizationId, TrainingIncidentId IncidentId, Guid OperationId, string? Note, UserId ActorUserId) : ICommand<TrainingIncidentResponse>;

public interface ITrainingIncidentExecutionLock
{
    Task AcquireAsync(OrganizationId organizationId, TrainingIncidentId incidentId, CancellationToken cancellationToken = default);
}
