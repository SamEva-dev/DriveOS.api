using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Application.Incidents;

public sealed record TrainingIncidentParticipantInput(int Type, Guid? ReferenceId, string? Label);
public sealed record TrainingIncidentParticipantResponse(Guid Id, int Type, Guid? ReferenceId, string? Label);
public sealed record TrainingIncidentEvidenceResponse(Guid Id, Guid DocumentId, string EvidenceType, string? Description, Guid AddedByUserId, DateTimeOffset AddedAtUtc);
public sealed record TrainingIncidentHistoryResponse(Guid Id, Guid OperationId, int Action, int FromStatus, int ToStatus, string? Reason, Guid ActorUserId, DateTimeOffset OccurredAtUtc);

public sealed record TrainingIncidentResponse(
    Guid Id,
    Guid OrganizationId,
    Guid TrainingSessionId,
    Guid StudentId,
    Guid InstructorId,
    Guid? VehicleId,
    Guid? BranchId,
    Guid PerformingOrganizationId,
    int IncidentType,
    int Severity,
    int Status,
    DateTimeOffset OccurredAtUtc,
    string Description,
    string ImmediateActions,
    bool EscalationRequired,
    bool RequiresFleetFollowUp,
    bool RequiresComplianceFollowUp,
    DateTimeOffset? EscalatedAtUtc,
    Guid? EscalatedByUserId,
    string? Resolution,
    DateTimeOffset? ResolvedAtUtc,
    Guid? ResolvedByUserId,
    DateTimeOffset? ClosedAtUtc,
    Guid? ClosedByUserId,
    IReadOnlyCollection<TrainingIncidentParticipantResponse> Participants,
    IReadOnlyCollection<TrainingIncidentEvidenceResponse> Evidence,
    IReadOnlyCollection<TrainingIncidentHistoryResponse> History,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);

public interface ITrainingIncidentReadService
{
    Task<TrainingIncidentResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingIncidentId incidentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TrainingIncidentResponse>> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default);
}

public static class TrainingIncidentMappings
{
    public static TrainingIncidentResponse ToResponse(TrainingIncident x) => new(
        x.Id.Value, x.OrganizationId.Value, x.TrainingSessionId.Value, x.StudentId.Value, x.InstructorId.Value,
        x.VehicleId, x.BranchId?.Value, x.PerformingOrganizationId.Value, (int)x.IncidentType, (int)x.Severity, (int)x.Status,
        x.OccurredAtUtc, x.Description, x.ImmediateActions, x.EscalationRequired, x.RequiresFleetFollowUp, x.RequiresComplianceFollowUp,
        x.EscalatedAtUtc, x.EscalatedByUserId?.Value, x.Resolution, x.ResolvedAtUtc, x.ResolvedByUserId?.Value,
        x.ClosedAtUtc, x.ClosedByUserId?.Value,
        x.Participants.Select(p => new TrainingIncidentParticipantResponse(p.Id.Value, (int)p.Type, p.ReferenceId, p.Label)).ToArray(),
        x.Evidence.OrderBy(e=>e.AddedAtUtc).Select(e => new TrainingIncidentEvidenceResponse(e.Id.Value,e.DocumentId,e.EvidenceType,e.Description,e.AddedByUserId.Value,e.AddedAtUtc)).ToArray(),
        x.History.OrderBy(h=>h.OccurredAtUtc).Select(h => new TrainingIncidentHistoryResponse(h.Id.Value,h.OperationId,(int)h.Action,(int)h.FromStatus,(int)h.ToStatus,h.Reason,h.ActorUserId.Value,h.OccurredAtUtc)).ToArray(),
        x.CreatedAtUtc, x.LastModifiedAtUtc);
}
