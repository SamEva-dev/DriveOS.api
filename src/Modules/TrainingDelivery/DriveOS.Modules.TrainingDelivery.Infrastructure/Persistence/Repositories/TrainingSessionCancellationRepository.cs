using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Repositories;

internal sealed class TrainingSessionCancellationRepository(TrainingDeliveryDbContext db)
    : ITrainingSessionCancellationRepository
{
    public Task<SessionCancellation?> GetByIdAsync(
        OrganizationId organizationId,
        SessionCancellationId cancellationId,
        CancellationToken cancellationToken = default) =>
        db.SessionCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == cancellationId,
                cancellationToken);

    public Task<SessionCancellation?> GetByIdForUpdateAsync(
        OrganizationId organizationId,
        SessionCancellationId cancellationId,
        CancellationToken cancellationToken = default) =>
        db.SessionCancellations
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == cancellationId,
                cancellationToken);

    public Task<SessionCancellation?> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default) =>
        db.SessionCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.TrainingSessionId == sessionId,
                cancellationToken);

    public Task<SessionCancellation?> GetBySessionForUpdateAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default) =>
        db.SessionCancellations
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.TrainingSessionId == sessionId,
                cancellationToken);

    public void Add(SessionCancellation cancellation) => db.SessionCancellations.Add(cancellation);
}
