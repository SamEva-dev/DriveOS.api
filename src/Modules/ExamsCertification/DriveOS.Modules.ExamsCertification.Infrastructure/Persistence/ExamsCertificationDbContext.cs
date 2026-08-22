using DriveOS.Modules.ExamsCertification.Application.Persistence;
using DriveOS.Modules.ExamsCertification.Domain.Readiness;
using DriveOS.Modules.ExamsCertification.Domain.Readiness.Opinions;
using DriveOS.Modules.ExamsCertification.Domain.Places;
using DriveOS.Modules.ExamsCertification.Domain.Places.Watch;
using DriveOS.Modules.ExamsCertification.Domain.Providers.Connections;
using DriveOS.Modules.ExamsCertification.Domain.Registrations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.File;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Submissions;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Convocations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Operations;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Assignments;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Preparation;
using DriveOS.Modules.ExamsCertification.Domain.Registrations.Attempts;
using DriveOS.Modules.ExamsCertification.Domain.Results;
using DriveOS.Modules.ExamsCertification.Domain.Results.Success;
using DriveOS.Modules.ExamsCertification.Domain.Results.Failure;
using DriveOS.Modules.ExamsCertification.Domain.Remediation;
using DriveOS.Modules.ExamsCertification.Domain.Certifications;
using DriveOS.Modules.ExamsCertification.Infrastructure.Success;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriveOS.Modules.ExamsCertification.Infrastructure.Persistence;

public sealed class ExamsCertificationDbContext(DbContextOptions<ExamsCertificationDbContext> options)
    : DbContext(options), IExamsCertificationUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public DbSet<ExamReadinessDecision> ExamReadinessDecisions => Set<ExamReadinessDecision>();
    public DbSet<ExamReadinessOpinion> ExamReadinessOpinions => Set<ExamReadinessOpinion>();
    public DbSet<ExamCenter> ExamCenters => Set<ExamCenter>();
    public DbSet<ExamPlace> ExamPlaces => Set<ExamPlace>();
    public DbSet<ExamPlaceWatchSubscription> ExamPlaceWatchSubscriptions => Set<ExamPlaceWatchSubscription>();
    public DbSet<ExamPlaceWatchScan> ExamPlaceWatchScans => Set<ExamPlaceWatchScan>();
    public DbSet<ExamPlaceWatchHit> ExamPlaceWatchHits => Set<ExamPlaceWatchHit>();
    public DbSet<ExamProviderConnection> ExamProviderConnections => Set<ExamProviderConnection>();
    public DbSet<ExamRegistration> ExamRegistrations => Set<ExamRegistration>();
    public DbSet<ExamRegistrationFile> ExamRegistrationFiles => Set<ExamRegistrationFile>();
    public DbSet<ExamRegistrationSubmission> ExamRegistrationSubmissions => Set<ExamRegistrationSubmission>();
    public DbSet<ExamConvocation> ExamConvocations => Set<ExamConvocation>();
    public DbSet<ExamConvocationRevision> ExamConvocationRevisions => Set<ExamConvocationRevision>();
    public DbSet<ExamOperationalPlan> ExamOperationalPlans => Set<ExamOperationalPlan>();
    public DbSet<ExamResourceAssignment> ExamResourceAssignments => Set<ExamResourceAssignment>();
    public DbSet<ExamPreparation> ExamPreparations => Set<ExamPreparation>();
    public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
    public DbSet<ExamResult> ExamResults => Set<ExamResult>();
    public DbSet<ExamResultRevision> ExamResultRevisions => Set<ExamResultRevision>();
    public DbSet<ExamSuccessProcess> ExamSuccessProcesses => Set<ExamSuccessProcess>();
    public DbSet<ExamFailureAnalysis> ExamFailureAnalyses => Set<ExamFailureAnalysis>();
    public DbSet<ExamRemediationRequest> ExamRemediationRequests => Set<ExamRemediationRequest>();
    public DbSet<ExamAttestation> ExamAttestations => Set<ExamAttestation>();
    public DbSet<ExamAttestationRevision> ExamAttestationRevisions => Set<ExamAttestationRevision>();
    internal DbSet<ExamSuccessConsequenceMessage> ExamSuccessConsequences => Set<ExamSuccessConsequenceMessage>();

    public bool HasActiveTransaction => _transaction is not null;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(ExamsCertificationSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExamsCertificationDbContext).Assembly);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);

        if (_transaction is null)
            return;

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
        ChangeTracker.Clear();
    }
}
