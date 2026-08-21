using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Repositories;

internal sealed class GroupTrainingSessionRepository(TrainingDeliveryDbContext db)
    : IGroupTrainingSessionRepository
{
    public Task<GroupTrainingSession?> GetByIdAsync(
        OrganizationId organizationId,
        GroupTrainingSessionId id,
        CancellationToken cancellationToken = default) =>
        ReadQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Id == id,
            cancellationToken);

    public Task<GroupTrainingSession?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        GroupTrainingSessionId id,
        CancellationToken cancellationToken = default) =>
        MutationQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.Id == id,
            cancellationToken);

    public Task<GroupTrainingSession?> GetBySourceBookingAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default) =>
        ReadQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.SourceBookingId == bookingId,
            cancellationToken);

    public Task<GroupTrainingSession?> GetBySourceBookingForUpdateAsync(
        OrganizationId organizationId,
        BookingId bookingId,
        CancellationToken cancellationToken = default) =>
        MutationQuery().SingleOrDefaultAsync(
            x => x.OrganizationId == organizationId && x.SourceBookingId == bookingId,
            cancellationToken);

    public void Add(GroupTrainingSession session) => db.GroupTrainingSessions.Add(session);

    private IQueryable<GroupTrainingSession> ReadQuery() => IncludeGraph(db.GroupTrainingSessions.AsNoTracking());

    private IQueryable<GroupTrainingSession> MutationQuery() => IncludeGraph(db.GroupTrainingSessions);

    private static IQueryable<GroupTrainingSession> IncludeGraph(IQueryable<GroupTrainingSession> query) =>
        query
            .Include(x => x.Participants)
            .Include(x => x.Operations);
}
