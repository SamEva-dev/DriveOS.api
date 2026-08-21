using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Repositories;

internal sealed class TrainingSessionRepository(TrainingDeliveryDbContext db) : ITrainingSessionRepository
{
    public Task<TrainingSession?> GetByIdAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default) =>
        ReadQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Id == sessionId,
            cancellationToken);

    public Task<TrainingSession?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default) =>
        MutationQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Id == sessionId,
            cancellationToken);

    public Task<TrainingSession?> GetBySourceBookingAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default) =>
        ReadQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.SourceBookingId == bookingId,
            cancellationToken);

    public Task<TrainingSession?> GetBySourceBookingForUpdateAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default) =>
        MutationQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.SourceBookingId == bookingId,
            cancellationToken);

    public void Add(TrainingSession session) => db.TrainingSessions.Add(session);

    private IQueryable<TrainingSession> ReadQuery() =>
        IncludeExecutionGraph(db.TrainingSessions.AsNoTracking());

    private IQueryable<TrainingSession> MutationQuery() =>
        IncludeExecutionGraph(db.TrainingSessions);

    private static IQueryable<TrainingSession> IncludeExecutionGraph(IQueryable<TrainingSession> query) =>
        query
            .Include(x => x.AttendanceHistory)
            .Include(x => x.Interventions)
            .Include(x => x.Observations)
            .Include(x => x.Interruptions)
            .Include(x => x.OdometerReadings)
            .Include(x => x.CompetencyAssessments)
            .Include(x => x.Report)
                .ThenInclude(x => x!.NarrativeRevisions)
            .Include(x => x.Report)
                .ThenInclude(x => x!.Revisions)
            .AsSplitQuery();
}
