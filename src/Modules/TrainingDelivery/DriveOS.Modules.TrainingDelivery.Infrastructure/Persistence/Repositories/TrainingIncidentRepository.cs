using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Repositories;

internal sealed class TrainingIncidentRepository(TrainingDeliveryDbContext db) : ITrainingIncidentRepository
{
    public Task<TrainingIncident?> GetByIdAsync(
        OrganizationId organizationId,
        TrainingIncidentId incidentId,
        CancellationToken cancellationToken = default) =>
        ReadQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Id == incidentId,
            cancellationToken);

    public Task<TrainingIncident?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        TrainingIncidentId incidentId,
        CancellationToken cancellationToken = default) =>
        MutationQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Id == incidentId,
            cancellationToken);

    public Task<TrainingIncident?> GetByReportOperationAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        Guid operationId,
        CancellationToken cancellationToken = default) =>
        ReadQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId
                && x.TrainingSessionId == sessionId
                && x.ReportOperationId == operationId,
            cancellationToken);

    public void Add(TrainingIncident incident) => db.TrainingIncidents.Add(incident);

    private IQueryable<TrainingIncident> ReadQuery() =>
        IncludeGraph(db.TrainingIncidents.AsNoTracking());

    private IQueryable<TrainingIncident> MutationQuery() =>
        IncludeGraph(db.TrainingIncidents);

    private static IQueryable<TrainingIncident> IncludeGraph(IQueryable<TrainingIncident> query) =>
        query
            .Include(x => x.Participants)
            .Include(x => x.Evidence)
            .Include(x => x.History)
            .AsSplitQuery();
}
