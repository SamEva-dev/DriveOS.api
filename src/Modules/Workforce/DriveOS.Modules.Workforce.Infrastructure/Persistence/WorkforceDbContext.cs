using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.LeavePolicies;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.Modules.Workforce.Domain.WorkingTime;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.Modules.Workforce.Domain.EquipmentAssignments;
using DriveOS.Modules.Workforce.Domain.PerformanceReviews;
using DriveOS.Modules.Workforce.Domain.EmployeeDocuments;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using Microsoft.EntityFrameworkCore;
namespace DriveOS.Modules.Workforce.Infrastructure.Persistence;
public sealed class WorkforceDbContext(DbContextOptions<WorkforceDbContext> options) : DbContext(options), IWorkforceUnitOfWork
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<WorkingTimePolicy> WorkingTimePolicies => Set<WorkingTimePolicy>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<EquipmentAssignment> EquipmentAssignments => Set<EquipmentAssignment>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<ProfessionalRestriction> ProfessionalRestrictions => Set<ProfessionalRestriction>();
    public DbSet<OffboardingProcess> OffboardingProcesses => Set<OffboardingProcess>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) { modelBuilder.HasDefaultSchema(WorkforceSchema.Name); modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkforceDbContext).Assembly); }
    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => SaveChangesAsync(cancellationToken);
}
