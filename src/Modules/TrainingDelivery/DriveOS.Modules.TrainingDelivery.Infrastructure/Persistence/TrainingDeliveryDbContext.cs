using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Consequences;
using DriveOS.Modules.TrainingDelivery.Infrastructure.CancellationConsequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;

public sealed class TrainingDeliveryDbContext(DbContextOptions<TrainingDeliveryDbContext> options) : DbContext(options), ITrainingDeliveryUnitOfWork
{
    private IDbContextTransaction? transaction;
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<GroupTrainingSession> GroupTrainingSessions => Set<GroupTrainingSession>();
    public DbSet<GroupTrainingSessionParticipant> GroupTrainingSessionParticipants => Set<GroupTrainingSessionParticipant>();
    public DbSet<GroupTrainingSessionOperation> GroupTrainingSessionOperations => Set<GroupTrainingSessionOperation>();
    public DbSet<SessionAttendance> SessionAttendance => Set<SessionAttendance>();
    public DbSet<SessionIntervention> SessionInterventions => Set<SessionIntervention>();
    public DbSet<SessionObservation> SessionObservations => Set<SessionObservation>();
    public DbSet<SessionMarker> SessionMarkers => Set<SessionMarker>();
    public DbSet<SessionInterruption> SessionInterruptions => Set<SessionInterruption>();
    public DbSet<SessionOdometerReading> SessionOdometerReadings => Set<SessionOdometerReading>();
    public DbSet<SessionReport> SessionReports => Set<SessionReport>();
    public DbSet<SessionReportNarrativeRevision> SessionReportNarrativeRevisions => Set<SessionReportNarrativeRevision>();
    public DbSet<SessionReportRevision> SessionReportRevisions => Set<SessionReportRevision>();
    public DbSet<SessionCompetencyAssessment> SessionCompetencyAssessments => Set<SessionCompetencyAssessment>();
    public DbSet<TrainingIncident> TrainingIncidents => Set<TrainingIncident>();
    public DbSet<SessionCancellation> SessionCancellations => Set<SessionCancellation>();
    public DbSet<TrainingIncidentParticipant> TrainingIncidentParticipants => Set<TrainingIncidentParticipant>();
    public DbSet<TrainingIncidentEvidence> TrainingIncidentEvidence => Set<TrainingIncidentEvidence>();
    public DbSet<TrainingIncidentHistoryEntry> TrainingIncidentHistory => Set<TrainingIncidentHistoryEntry>();
    internal DbSet<TrainingSessionCompletionConsequenceMessage> TrainingSessionCompletionConsequences => Set<TrainingSessionCompletionConsequenceMessage>();
    internal DbSet<TrainingSessionCancellationConsequenceMessage> TrainingSessionCancellationConsequences => Set<TrainingSessionCancellationConsequenceMessage>();
    public bool HasActiveTransaction => transaction is not null;
    protected override void OnModelCreating(ModelBuilder modelBuilder) { modelBuilder.HasDefaultSchema(TrainingDeliverySchema.Name); modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrainingDeliveryDbContext).Assembly); }
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default) { transaction ??= await Database.BeginTransactionAsync(cancellationToken); }
    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default) { await SaveChangesAsync(cancellationToken); if (transaction is not null) { await transaction.CommitAsync(cancellationToken); await transaction.DisposeAsync(); transaction = null; } }
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default) { if (transaction is not null) { await transaction.RollbackAsync(cancellationToken); await transaction.DisposeAsync(); transaction = null; ChangeTracker.Clear(); } }
}
