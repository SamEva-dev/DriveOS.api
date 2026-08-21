using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;

internal sealed class TrainingSessionExecutionLock(TrainingDeliveryDbContext db) : ITrainingSessionExecutionLock
{
    public async Task AcquireAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!db.HasActiveTransaction)
            throw new InvalidOperationException("A Training Delivery transaction must be active before acquiring a session execution lock.");

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM training_delivery.training_sessions WHERE \"OrganizationId\" = {organizationId.Value} AND \"Id\" = {sessionId.Value} FOR UPDATE",
            cancellationToken);
    }
}
