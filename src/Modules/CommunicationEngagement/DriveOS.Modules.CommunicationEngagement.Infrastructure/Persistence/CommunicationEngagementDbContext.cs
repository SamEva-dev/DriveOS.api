using DriveOS.Modules.CommunicationEngagement.Application.Persistence;
using DriveOS.Modules.CommunicationEngagement.Domain.Conversations;
using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.Modules.CommunicationEngagement.Domain.Surveys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Persistence;
public sealed class CommunicationEngagementDbContext(DbContextOptions<CommunicationEngagementDbContext> options):DbContext(options),ICommunicationEngagementUnitOfWork
{
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)=>
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
    public DbSet<Conversation> Conversations=>Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages=>Set<ConversationMessage>();
    public DbSet<CommunicationNotification> Notifications=>Set<CommunicationNotification>();
    public DbSet<NotificationPreference> NotificationPreferences=>Set<NotificationPreference>();
    public DbSet<CommunicationSurveyRequest> SurveyRequests=>Set<CommunicationSurveyRequest>();
    protected override void OnModelCreating(ModelBuilder modelBuilder){modelBuilder.HasDefaultSchema(CommunicationEngagementSchema.Name);modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommunicationEngagementDbContext).Assembly);ApplyUserIdConversions(modelBuilder);}
    public Task<int> CommitAsync(CancellationToken cancellationToken=default)=>SaveChangesAsync(cancellationToken);
    private static void ApplyUserIdConversions(ModelBuilder modelBuilder)
    {
        var required=new ValueConverter<UserId,Guid>(x=>x.Value,x=>new UserId(x));
        var optional=new ValueConverter<UserId?,Guid?>(x=>x.HasValue?x.Value.Value:null,x=>x.HasValue?new UserId(x.Value):null);
        foreach(var property in modelBuilder.Model.GetEntityTypes().SelectMany(x=>x.GetProperties()))
        {
            if(property.GetValueConverter() is not null)continue;
            if(property.ClrType==typeof(UserId))property.SetValueConverter(required);
            else if(property.ClrType==typeof(UserId?))property.SetValueConverter(optional);
        }
    }
    private sealed class UserIdConverter():ValueConverter<UserId,Guid>(x=>x.Value,x=>new UserId(x));
}
