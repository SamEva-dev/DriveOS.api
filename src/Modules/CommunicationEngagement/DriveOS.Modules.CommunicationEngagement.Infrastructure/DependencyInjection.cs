using DriveOS.Modules.CommunicationEngagement.Infrastructure.Surveys;
using DriveOS.Modules.CommunicationEngagement.Application.Surveys;
using DriveOS.Modules.CommunicationEngagement.Domain.Surveys;
using DriveOS.Modules.CommunicationEngagement.Application.Persistence;
using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.Modules.CommunicationEngagement.Application.Notifications;
using DriveOS.Modules.CommunicationEngagement.Infrastructure.Notifications;
using DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence;
using DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace DriveOS.Modules.CommunicationEngagement.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddCommunicationEngagementInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        string cs=configuration.GetConnectionString("DriveOS")??throw new InvalidOperationException("The DriveOS database connection string is missing.");
        services.AddDbContext<CommunicationEngagementDbContext>(options=>options.UseNpgsql(cs,npgsql=>npgsql.MigrationsHistoryTable("__ef_migrations_history",CommunicationEngagementSchema.Name)));
        services.AddScoped<ICommunicationEngagementUnitOfWork>(sp=>sp.GetRequiredService<CommunicationEngagementDbContext>());
        services.AddScoped<IConversationRepository,ConversationRepository>();
        services.AddScoped<IConversationMessageRepository,ConversationMessageRepository>();
        services.AddScoped<ICommunicationNotificationRepository,CommunicationNotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository,NotificationPreferenceRepository>();
        services.AddScoped<ICommunicationSurveyRequestRepository,CommunicationSurveyRequestRepository>();
        services.AddScoped<ICommunicationNotificationWriter,CommunicationNotificationWriter>();
        services.AddScoped<ICommunicationNotificationReadService,CommunicationNotificationReadService>();
        services.AddScoped<ICommunicationSurveyRequestWriter,CommunicationSurveyRequestWriter>();
        return services;
    }
}
