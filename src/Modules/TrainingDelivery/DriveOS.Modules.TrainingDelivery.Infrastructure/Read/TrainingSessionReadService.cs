using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Read;

internal sealed class TrainingSessionReadService(TrainingDeliveryDbContext db) : ITrainingSessionReadService
{
    public async Task<TrainingSessionResponse?> GetAsync(
        OrganizationId organizationId,
        TrainingSessionId sessionId,
        CancellationToken cancellationToken = default)
    {
        TrainingSession? session = await db.TrainingSessions
            .AsNoTracking()
            .Include(x => x.AttendanceHistory)
            .Include(x => x.Interventions)
            .Include(x => x.Observations)
            .Include(x => x.Interruptions)
            .Include(x => x.OdometerReadings)
            .Include(x => x.CompetencyAssessments)
            .Include(x => x.Report)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                x => x.OrganizationId == organizationId && x.Id == sessionId,
                cancellationToken);

        return session is null ? null : TrainingSessionMappings.ToResponse(session);
    }
}
