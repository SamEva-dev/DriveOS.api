using System.Text.Json;
using DriveOS.Modules.CommunicationEngagement.Application.Notifications;
using DriveOS.Modules.CommunicationEngagement.Application.Persistence;
using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.CommunicationEngagement.Infrastructure.Notifications;

internal sealed class CommunicationNotificationWriter(
    ICommunicationNotificationRepository repository,
    INotificationPreferenceRepository preferences,
    ICommunicationNotificationEmailGateway email,
    ICommunicationEngagementUnitOfWork uow):ICommunicationNotificationWriter
{
    public async Task<Guid> EnqueueAsync(
        EnqueueCommunicationNotificationRequest request,
        CancellationToken cancellationToken=default)
    {
        if(await repository.ExistsByDeduplicationKeyAsync(request.DeduplicationKey,cancellationToken))
            return Guid.Empty;

        var recipientType=Enum.Parse<CommunicationNotificationRecipientType>(request.RecipientType,true);

        bool inAppEnabled=true;
        bool emailEnabled=true;

        if(recipientType==CommunicationNotificationRecipientType.User)
        {
            var pref=await preferences.GetAsync(
                new UserId(request.RecipientId),
                request.Category,
                false,
                cancellationToken);

            if(pref is not null)
            {
                inAppEnabled=pref.InAppEnabled;
                emailEnabled=pref.EmailEnabled;
            }
        }

        var id=new CommunicationNotificationId(Guid.NewGuid());
        var created=CommunicationNotification.Create(
            id,
            recipientType,
            request.RecipientId,
            request.OrganizationId,
            request.Category,
            request.TemplateKey,
            request.DeduplicationKey,
            JsonSerializer.Serialize(request.Parameters),
            request.RelatedEntityType,
            request.RelatedEntityId,
            request.EmailAddress,
            request.CultureCode,
            inAppEnabled,
            DateTimeOffset.UtcNow,
            request.ActorUserId);

        if(created.IsFailure)
            throw new InvalidOperationException($"{created.Error.Code}:{created.Error.MessageKey}");

        repository.Add(created.Value);
        await uow.CommitAsync(cancellationToken);

        if(!emailEnabled||string.IsNullOrWhiteSpace(request.EmailAddress))
        {
            created.Value.MarkEmailSkipped(DateTimeOffset.UtcNow);
            await uow.CommitAsync(cancellationToken);
            return id.Value;
        }

        Guid? emailId=await email.TryQueueAsync(
            new CommunicationNotificationEmailRequest(
                request.EmailAddress,
                string.IsNullOrWhiteSpace(request.CultureCode)?"fr":request.CultureCode,
                request.TemplateKey,
                request.Parameters),
            cancellationToken);

        if(emailId.HasValue)
            created.Value.MarkEmailQueued(emailId.Value,DateTimeOffset.UtcNow);
        else
            created.Value.MarkEmailSkipped(DateTimeOffset.UtcNow);

        await uow.CommitAsync(cancellationToken);
        return id.Value;
    }
}

internal sealed class CommunicationNotificationReadService(
    ICommunicationNotificationRepository repository):ICommunicationNotificationReadService
{
    public async Task<IReadOnlyList<CommunicationNotificationListItem>> ListForUserAsync(
        UserId userId,int take=50,bool unreadOnly=false,CancellationToken cancellationToken=default)
    {
        IReadOnlyList<CommunicationNotification> items=
            await repository.ListForUserAsync(userId,take,unreadOnly,cancellationToken);

        return items.Select(x=>new CommunicationNotificationListItem(
            x.Id.Value,
            x.Category,
            x.TemplateKey,
            JsonSerializer.Deserialize<Dictionary<string,string?>>(x.PayloadJson)??new(),
            x.RelatedEntityType,
            x.RelatedEntityId,
            x.Status.ToString(),
            x.EmailStatus.ToString(),
            x.CreatedAtUtc,
            x.ReadAtUtc)).ToArray();
    }

    public Task<int> CountUnreadAsync(UserId userId,CancellationToken cancellationToken=default)=>
        repository.CountUnreadAsync(userId,cancellationToken);
}
