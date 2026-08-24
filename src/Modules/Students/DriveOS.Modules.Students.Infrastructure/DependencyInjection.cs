using DriveOS.Modules.Students.Application.Abstractions.Persistence;
using DriveOS.Modules.Students.Application.Administration;
using DriveOS.Modules.Students.Application.Branches;
using DriveOS.Modules.Students.Application.Checklists;
using DriveOS.Modules.Students.Application.Closures;
using DriveOS.Modules.Students.Application.Dashboard.GetDashboard;
using DriveOS.Modules.Students.Application.Documents;
using DriveOS.Modules.Students.Application.Enrollments.StartDirectEnrollment;
using DriveOS.Modules.Students.Application.ExternalTransfers;
using DriveOS.Modules.Students.Application.Guardians;
using DriveOS.Modules.Students.Application.Instructors;
using DriveOS.Modules.Students.Application.Provisioning;
using DriveOS.Modules.Students.Application.Reactivations;
using DriveOS.Modules.Students.Application.Relationships;
using DriveOS.Modules.Students.Application.RegulatoryIdentities;
using DriveOS.Modules.Students.Application.Statuses;
using DriveOS.Modules.Students.Application.Students.GetStudentOverview;
using DriveOS.Modules.Students.Application.Students.GetStudents;
using DriveOS.Modules.Students.Application.Students.Identity;
using DriveOS.Modules.Students.Application.Suspensions;
using DriveOS.Modules.Students.Application.Transfers;
using DriveOS.Modules.Students.Infrastructure.Administration;
using DriveOS.Modules.Students.Infrastructure.Branches;
using DriveOS.Modules.Students.Infrastructure.Checklists;
using DriveOS.Modules.Students.Infrastructure.Closures;
using DriveOS.Modules.Students.Infrastructure.Documents;
using DriveOS.Modules.Students.Infrastructure.Enrollments;
using DriveOS.Modules.Students.Infrastructure.ExternalTransfers;
using DriveOS.Modules.Students.Infrastructure.Guardians;
using DriveOS.Modules.Students.Infrastructure.Instructors;
using DriveOS.Modules.Students.Infrastructure.Persistence;
using DriveOS.Modules.Students.Infrastructure.Persistence.Interceptors;
using DriveOS.Modules.Students.Infrastructure.Persistence.Queries;
using DriveOS.Modules.Students.Infrastructure.Provisioning;
using DriveOS.Modules.Students.Infrastructure.Reactivations;
using DriveOS.Modules.Students.Infrastructure.Relationships;
using DriveOS.Modules.Students.Infrastructure.RegulatoryIdentities;
using DriveOS.Modules.Students.Infrastructure.Statuses;
using DriveOS.Modules.Students.Infrastructure.Students;
using DriveOS.Modules.Students.Infrastructure.Suspensions;
using DriveOS.Modules.Students.Infrastructure.Transfers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.Students.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudentsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        string connectionString =
            configuration.GetConnectionString("DriveOS")
            ?? throw new InvalidOperationException(
                "The DriveOS database connection string is missing."
            );
        services.AddScoped<StudentsAuditableEntityInterceptor>();
        services.AddDbContext<StudentsDbContext>(
            (provider, options) =>
            {
                options.UseNpgsql(
                    connectionString,
                    npgsql =>
                        npgsql.MigrationsHistoryTable(
                            "__ef_migrations_history",
                            StudentsSchema.Name
                        )
                );
                options.AddInterceptors(
                    provider.GetRequiredService<StudentsAuditableEntityInterceptor>()
                );
            }
        );
        services.AddScoped<IStudentsUnitOfWork>(p => p.GetRequiredService<StudentsDbContext>());
        services.AddScoped<IStudentDashboardReadService, StudentDashboardReadService>();
        services.AddScoped<IStudentProvisioningService, StudentProvisioningService>();
        services.AddScoped<IStudentReadService, StudentReadService>();
        services.AddScoped<IDirectEnrollmentService, DirectEnrollmentService>();
        services.AddScoped<IStudentOverviewReadService, StudentOverviewReadService>();
        services.AddScoped<IStudentIdentityService, StudentIdentityService>();
        services.AddScoped<StudentRegulatoryIdentityService>();
        services.AddScoped<IStudentRegulatoryIdentityService>(p => p.GetRequiredService<StudentRegulatoryIdentityService>());
        services.AddScoped<IStudentRegulatoryIdentityReadService>(p => p.GetRequiredService<StudentRegulatoryIdentityService>());
        services.AddScoped<IAdministrationService, AdministrationService>();
        services.AddScoped<IGuardianService, GuardianService>();
        services.AddScoped<IStudentRelationshipService, StudentRelationshipService>();
        services.AddScoped<IEnrollmentPrerequisiteSnapshotProvider, NullEnrollmentPrerequisiteSnapshotProvider>();
        services.AddScoped<IEnrollmentChecklistService, EnrollmentChecklistService>();
        services.AddScoped<IStudentDocumentService, StudentDocumentService>();
        services.AddScoped<IStudentStatusService, StudentStatusService>();
        services.AddScoped<IStudentBranchService, StudentBranchManagementService>();
        services.AddScoped<IStudentBranchVerifier, StudentBranchVerifier>();
        services.AddScoped<IStudentBranchImpactAnalyzer, StudentBranchImpactAnalyzer>();
        services.AddScoped<IStudentInstructorService, StudentInstructorManagementService>();
        services.AddScoped<IInstructorEligibilityGateway, InstructorEligibilityGateway>();
        services.AddScoped<IInstructorWorkforceEligibilityGateway, NullInstructorWorkforceEligibilityGateway>();
        services.AddScoped<IInternalTransferService, InternalTransferService>();
        services.AddScoped<IInternalTransferImpactAnalyzer, InternalTransferImpactAnalyzer>();
        services.AddScoped<IExternalTransferService, ExternalTransferService>();
        services.AddScoped<
            IExternalTransferPreconditionGateway,
            ExternalTransferPreconditionGateway
        >();
        services.AddScoped<IEnrollmentSuspensionService, EnrollmentSuspensionService>();
        services.AddHostedService<EnrollmentSuspensionScheduler>();
        services.AddScoped<IEnrollmentReactivationService, EnrollmentReactivationService>();
        services.AddHostedService<EnrollmentReactivationScheduler>();
        services.AddScoped<IEnrollmentClosureService, EnrollmentClosureService>();
        services.AddHostedService<InternalTransferScheduler>();
        services.AddSingleton<IStudentDocumentStorage, EncryptedStudentDocumentStorage>();
        services.AddSingleton<
            IStudentDocumentSecurityScanner,
            ClamAvStudentDocumentSecurityScanner
        >();
        return services;
    }
}
