using DriveOS.Modules.Workforce.Infrastructure.AccessRevocation;
using DriveOS.Modules.Workforce.Infrastructure.Read;
using DriveOS.Modules.Workforce.Application.Availability;
using DriveOS.Modules.Workforce.Application.Persistence;
using DriveOS.Modules.Workforce.Application.Qualifications;
using DriveOS.Modules.Workforce.Application.ProfessionalEligibility;
using DriveOS.Modules.Workforce.Domain.Employees;
using DriveOS.Modules.Workforce.Domain.JobPositions;
using DriveOS.Modules.Workforce.Domain.LeavePolicies;
using DriveOS.Modules.Workforce.Domain.LeaveRequests;
using DriveOS.Modules.Workforce.Domain.Timesheets;
using DriveOS.Modules.Workforce.Domain.EquipmentAssignments;
using DriveOS.Modules.Workforce.Domain.PerformanceReviews;
using DriveOS.Modules.Workforce.Domain.EmployeeDocuments;
using DriveOS.Modules.Workforce.Domain.ProfessionalRestrictions;
using DriveOS.Modules.Workforce.Domain.Offboarding;
using DriveOS.Modules.Workforce.Application.Offboarding;
using DriveOS.Modules.Workforce.Domain.WorkingTime;
using DriveOS.Modules.Workforce.Infrastructure.Persistence;
using DriveOS.Modules.Workforce.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace DriveOS.Modules.Workforce.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddWorkforceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string cs = configuration.GetConnectionString("DriveOS") ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddDbContext<WorkforceDbContext>(options => options.UseNpgsql(cs, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", WorkforceSchema.Name)));
        services.AddScoped<IWorkforceUnitOfWork>(sp => sp.GetRequiredService<WorkforceDbContext>());
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IJobPositionRepository, JobPositionRepository>();
        services.AddScoped<ILeavePolicyRepository, LeavePolicyRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<ITimesheetRepository, TimesheetRepository>();
        services.AddScoped<IEquipmentAssignmentRepository, EquipmentAssignmentRepository>();
        services.AddScoped<IPerformanceReviewRepository, PerformanceReviewRepository>();
        services.AddScoped<IEmployeeDocumentRepository, EmployeeDocumentRepository>();
        services.AddScoped<IProfessionalRestrictionRepository, ProfessionalRestrictionRepository>();
        services.AddScoped<IOffboardingProcessRepository, OffboardingProcessRepository>();
        services.AddScoped<IWorkingTimePolicyRepository, WorkingTimePolicyRepository>();
        services.AddScoped<IWorkforceAvailabilityReadService, WorkforceAvailabilityReadService>();
        services.AddScoped<IWorkforceProfessionalEligibilityReadService, WorkforceProfessionalEligibilityReadService>();
        services.AddScoped<IOffboardingDependencyReadService, OffboardingDependencyReadService>();
        services.AddScoped<DriveOS.Modules.Workforce.Application.Dashboard.IWorkforceDashboardReadService, WorkforceDashboardReadService>();
        services.AddScoped<DriveOS.Modules.Workforce.Application.Analytics.IWorkforceAnalyticsReadService, WorkforceAnalyticsReadService>();
        services.AddScoped<IWorkforceInstructorAuthorizationReadService, WorkforceInstructorAuthorizationReadService>();
        services.Configure<AuthGateWorkforceAccessOptions>(configuration.GetSection(AuthGateWorkforceAccessOptions.SectionName));
        string? authGateBaseUrl = configuration["AuthGate:BaseUrl"];
        if (Uri.TryCreate(authGateBaseUrl, UriKind.Absolute, out Uri? authGateUri))
        {
            services.AddHttpClient<IEmployeeApplicationAccessRevoker, AuthGateEmployeeApplicationAccessRevoker>(client => client.BaseAddress = authGateUri);
        }
        else
        {
            services.AddScoped<IEmployeeApplicationAccessRevoker, DisabledEmployeeApplicationAccessRevoker>();
        }
        return services;
    }
}
