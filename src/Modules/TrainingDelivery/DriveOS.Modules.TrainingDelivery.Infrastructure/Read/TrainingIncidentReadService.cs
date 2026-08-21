using DriveOS.Modules.TrainingDelivery.Application.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Read;

internal sealed class TrainingIncidentReadService(TrainingDeliveryDbContext db) : ITrainingIncidentReadService
{
    public async Task<TrainingIncidentResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingIncidentId incidentId,
        CancellationToken cancellationToken = default)
    {
        TrainingIncident? incident = await Query()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == incidentId,
                cancellationToken);

        return incident is null ? null : TrainingIncidentMappings.ToResponse(incident);
    }

    public async Task<IReadOnlyCollection<TrainingIncidentResponse>> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        List<TrainingIncident> incidents = await Query()
            .Where(x => x.OrganizationId == organizationId && x.TrainingSessionId == sessionId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .ToListAsync(cancellationToken);

        return incidents.Select(TrainingIncidentMappings.ToResponse).ToArray();
    }

    private IQueryable<TrainingIncident> Query() =>
        db.TrainingIncidents
            .AsNoTracking()
            .Include(x => x.Participants)
            .Include(x => x.Evidence)
            .Include(x => x.History)
            .AsSplitQuery();
}
