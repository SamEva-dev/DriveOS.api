using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.TrainingDelivery.Domain.Incidents;

public interface ITrainingIncidentRepository
{
    Task<TrainingIncident?> GetByIdAsync(
        OrganizationId organizationId,
        TrainingIncidentId incidentId,
        CancellationToken cancellationToken = default);

    Task<TrainingIncident?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        TrainingIncidentId incidentId,
        CancellationToken cancellationToken = default);

    Task<TrainingIncident?> GetByReportOperationAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        Guid operationId,
        CancellationToken cancellationToken = default);

    void Add(TrainingIncident incident);
}
