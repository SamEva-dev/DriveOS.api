using DriveOS.Modules.Students.Application.Abstractions.Persistence;
using DriveOS.Modules.Students.Domain.Administration;
using DriveOS.Modules.Students.Domain.Branches;
using DriveOS.Modules.Students.Domain.Checklists;
using DriveOS.Modules.Students.Domain.Closures;
using DriveOS.Modules.Students.Domain.Documents;
using DriveOS.Modules.Students.Domain.Enrollments;
using DriveOS.Modules.Students.Domain.ExternalTransfers;
using DriveOS.Modules.Students.Domain.Guardians;
using DriveOS.Modules.Students.Domain.Instructors;
using DriveOS.Modules.Students.Domain.Relationships;
using DriveOS.Modules.Students.Domain.Statuses;
using DriveOS.Modules.Students.Domain.Students;
using DriveOS.Modules.Students.Domain.Suspensions;
using DriveOS.Modules.Students.Domain.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriveOS.Modules.Students.Infrastructure.Persistence;

public sealed class StudentsDbContext(DbContextOptions<StudentsDbContext> options)
    : DbContext(options),
        IStudentsUnitOfWork
{
    private IDbContextTransaction? currentTransaction;
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<StudentIdentityAuditEntry> StudentIdentityAuditEntries =>
        Set<StudentIdentityAuditEntry>();
    public DbSet<AdministrativeCase> AdministrativeCases => Set<AdministrativeCase>();
    public DbSet<GuardianRelationship> GuardianRelationships => Set<GuardianRelationship>();
    public DbSet<StudentRelationship> StudentRelationships => Set<StudentRelationship>();
    public DbSet<EnrollmentChecklist> EnrollmentChecklists => Set<EnrollmentChecklist>();
    public DbSet<EnrollmentChecklistRule> EnrollmentChecklistRules =>
        Set<EnrollmentChecklistRule>();
    public DbSet<StudentDocument> StudentDocuments => Set<StudentDocument>();
    public DbSet<StudentStatusBoard> StudentStatusBoards => Set<StudentStatusBoard>();
    public DbSet<StudentBranchPortfolio> StudentBranchPortfolios => Set<StudentBranchPortfolio>();
    public DbSet<StudentInstructorPortfolio> StudentInstructorPortfolios =>
        Set<StudentInstructorPortfolio>();
    public DbSet<InternalTransferCase> InternalTransferCases => Set<InternalTransferCase>();
    public DbSet<ExternalTransferCase> ExternalTransferCases => Set<ExternalTransferCase>();
    public DbSet<EnrollmentSuspension> EnrollmentSuspensions => Set<EnrollmentSuspension>();
    public DbSet<EnrollmentReactivation> EnrollmentReactivations => Set<EnrollmentReactivation>();
    public DbSet<EnrollmentClosureCase> EnrollmentClosures => Set<EnrollmentClosureCase>();
    public bool HasActiveTransaction => currentTransaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
            throw new InvalidOperationException("A transaction is already active.");
        currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
        SaveChangesAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            currentTransaction
            ?? throw new InvalidOperationException("No active transaction exists.");
        try
        {
            await SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction =
            currentTransaction
            ?? throw new InvalidOperationException("No active transaction exists.");
        try
        {
            await transaction.RollbackAsync(cancellationToken);
            ChangeTracker.Clear();
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(StudentsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudentsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override void Dispose()
    {
        currentTransaction?.Dispose();
        currentTransaction = null;
        base.Dispose();
    }

    public override async ValueTask DisposeAsync()
    {
        await DisposeTransactionAsync();
        await base.DisposeAsync();
    }

    private async ValueTask DisposeTransactionAsync()
    {
        if (currentTransaction is null)
            return;
        await currentTransaction.DisposeAsync();
        currentTransaction = null;
    }
}
