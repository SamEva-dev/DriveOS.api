using DriveOS.Application.Abstractions.Persistence;
using DriveOS.Modules.SchedulingCapacity.Application.Persistence;
using DriveOS.Modules.SchedulingCapacity.Domain.Availability;
using DriveOS.Modules.SchedulingCapacity.Domain.CalendarResources;
using DriveOS.Modules.SchedulingCapacity.Domain.Bookings;
using DriveOS.Modules.SchedulingCapacity.Domain.Recurrences;
using DriveOS.Modules.SchedulingCapacity.Domain.Conflicts;
using DriveOS.Modules.SchedulingCapacity.Domain.WaitingList;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DriveOS.Modules.SchedulingCapacity.Infrastructure.Persistence;

public sealed class SchedulingCapacityDbContext(DbContextOptions<SchedulingCapacityDbContext> options)
    : DbContext(options), ISchedulingCapacityUnitOfWork, IUnitOfWork
{
    private IDbContextTransaction? transaction;

    public DbSet<CalendarResource> CalendarResources => Set<CalendarResource>();
    public DbSet<AvailabilityPlan> AvailabilityPlans => Set<AvailabilityPlan>();
    public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
    public DbSet<AvailabilityException> AvailabilityExceptions => Set<AvailabilityException>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingResource> BookingResources => Set<BookingResource>();
    public DbSet<BookingParticipant> BookingParticipants => Set<BookingParticipant>();
    public DbSet<BookingRescheduleHistory> BookingRescheduleHistory => Set<BookingRescheduleHistory>();
    public DbSet<BookingCancellation> BookingCancellations => Set<BookingCancellation>();
    public DbSet<BookingAttendance> BookingAttendanceHistory => Set<BookingAttendance>();
    public DbSet<BookingInstructorReplacement> BookingInstructorReplacements => Set<BookingInstructorReplacement>();
    public DbSet<BookingVehicleReplacement> BookingVehicleReplacements => Set<BookingVehicleReplacement>();
    public DbSet<RecurrenceSeries> RecurrenceSeries => Set<RecurrenceSeries>();
    public DbSet<RecurrenceOccurrence> RecurrenceOccurrences => Set<RecurrenceOccurrence>();
    public DbSet<RecurrenceResource> RecurrenceResources => Set<RecurrenceResource>();
    public DbSet<SchedulingConflict> SchedulingConflicts => Set<SchedulingConflict>();
    public DbSet<WaitingListEntry> WaitingListEntries => Set<WaitingListEntry>();
    public DbSet<WaitingListProposal> WaitingListProposals => Set<WaitingListProposal>();

    public bool HasActiveTransaction => transaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is not null) throw new InvalidOperationException("A transaction is already active.");
        transaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is null) throw new InvalidOperationException("No active transaction exists.");
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
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction is null) throw new InvalidOperationException("No active transaction exists.");
        try
        {
            await transaction.RollbackAsync(cancellationToken);
            ChangeTracker.Clear();
        }
        finally
        {
            await transaction.DisposeAsync();
            transaction = null;
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchedulingCapacitySchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulingCapacityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
