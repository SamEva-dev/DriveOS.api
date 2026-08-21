using DriveOS.Modules.TrainingDelivery.Application.Cancellations;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Read;

internal sealed class TrainingSessionCancellationReadService(TrainingDeliveryDbContext db)
    : ITrainingSessionCancellationReadService
{
    public async Task<SessionCancellationResponse?> GetBySessionAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        SessionCancellation? cancellation = await db.SessionCancellations
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.TrainingSessionId == sessionId,
                cancellationToken);

        return cancellation is null ? null : SessionCancellationMappings.ToResponse(cancellation);
    }
}
