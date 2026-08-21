using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;

internal sealed class TrainingSessionMaterializationLock(TrainingDeliveryDbContext db) : ITrainingSessionMaterializationLock
{
    public Task AcquireAsync(OrganizationId organizationId, BookingId bookingId, CancellationToken cancellationToken = default)
    {
        if (!db.HasActiveTransaction) throw new InvalidOperationException("A transaction is required before acquiring the training-session materialization lock.");
        long key = BitConverter.ToInt64(System.Security.Cryptography.SHA256.HashData(organizationId.Value.ToByteArray().Concat(bookingId.Value.ToByteArray()).ToArray()), 0);
        return db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({key})", cancellationToken);
    }
}
