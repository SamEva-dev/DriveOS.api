using DriveOS.Modules.TrainingDelivery.Application.Persistence;
using DriveOS.Modules.TrainingDelivery.Application.Sessions;
using DriveOS.Modules.TrainingDelivery.Application.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Sessions;
using DriveOS.Modules.TrainingDelivery.Domain.Incidents;
using DriveOS.Modules.TrainingDelivery.Domain.Cancellations;
using DriveOS.Modules.TrainingDelivery.Domain.GroupSessions;
using DriveOS.Modules.TrainingDelivery.Application.Cancellations;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Persistence.Repositories;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Read;
using DriveOS.Modules.TrainingDelivery.Infrastructure.Consequences;
using DriveOS.Modules.TrainingDelivery.Application.Consequences;
using DriveOS.Modules.TrainingDelivery.Application.CancellationConsequences;
using DriveOS.Modules.TrainingDelivery.Infrastructure.CancellationConsequences;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DriveOS.Modules.TrainingDelivery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTrainingDeliveryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new TrainingSessionExecutionOptions
        {
            PreparationLeadMinutes = ReadInt(configuration, "TrainingDelivery:SessionExecution:PreparationLeadMinutes", 30),
            StartEarlyToleranceMinutes = ReadInt(configuration, "TrainingDelivery:SessionExecution:StartEarlyToleranceMinutes", 15),
            StartLateToleranceMinutes = ReadInt(configuration, "TrainingDelivery:SessionExecution:StartLateToleranceMinutes", 180),
            ReadinessValidityMinutes = ReadInt(configuration, "TrainingDelivery:SessionExecution:ReadinessValidityMinutes", 5)
        });
        services.AddSingleton(new TrainingSessionAttendanceOptions
        {
            RecordingEarlyToleranceMinutes = ReadInt(configuration, "TrainingDelivery:Attendance:RecordingEarlyToleranceMinutes", 30),
            CorrectionWindowHours = ReadInt(configuration, "TrainingDelivery:Attendance:CorrectionWindowHours", 24)
        });
        string cs = configuration.GetConnectionString("DriveOS") ?? throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddDbContext<TrainingDeliveryDbContext>(o => o.UseNpgsql(cs, n => n.MigrationsHistoryTable("__ef_migrations_history", TrainingDeliverySchema.Name)));
        services.AddScoped<ITrainingDeliveryUnitOfWork>(sp => sp.GetRequiredService<TrainingDeliveryDbContext>());
        services.AddScoped<ITrainingSessionRepository, TrainingSessionRepository>();
        services.AddScoped<IGroupTrainingSessionRepository, GroupTrainingSessionRepository>();
        services.AddScoped<ITrainingSessionMaterializationLock, TrainingSessionMaterializationLock>();
        services.AddScoped<ITrainingSessionExecutionLock, TrainingSessionExecutionLock>();
        services.AddScoped<ITrainingSessionReadService, TrainingSessionReadService>();
        services.AddScoped<ITrainingIncidentRepository, TrainingIncidentRepository>();
        services.AddScoped<ITrainingSessionCancellationRepository, TrainingSessionCancellationRepository>();
        services.AddScoped<ITrainingSessionCancellationReadService, TrainingSessionCancellationReadService>();
        services.AddScoped<ITrainingIncidentExecutionLock, TrainingIncidentExecutionLock>();
        services.AddScoped<ITrainingIncidentReadService, TrainingIncidentReadService>();
        services.AddScoped<ITrainingSessionCompletionConsequenceStore, TrainingSessionCompletionConsequenceStore>();
        services.AddHostedService<TrainingSessionCompletionConsequenceWorker>();
        services.AddScoped<ITrainingSessionCancellationConsequenceStore, TrainingSessionCancellationConsequenceStore>();
        services.AddHostedService<TrainingSessionCancellationConsequenceWorker>();
        return services;
    }

    private static int ReadInt(IConfiguration configuration, string key, int fallback) =>
        int.TryParse(configuration[key], out int value) ? value : fallback;
}
