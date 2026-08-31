using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Application.Abstractions.Time;
using DriveOS.Modules.CommunicationEngagement.Application.Persistence;
using DriveOS.Modules.CommunicationEngagement.Domain.Notifications;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Modules.CommunicationEngagement.Application.Notifications;

public sealed class MarkCommunicationNotificationReadCommandHandler(
    ICommunicationNotificationRepository notifications,
    ICommunicationEngagementUnitOfWork uow,
    IClock clock):ICommandHandler<MarkCommunicationNotificationReadCommand>
{
    public async Task<Result> Handle(MarkCommunicationNotificationReadCommand c,CancellationToken ct)
    {
        var notification=await notifications.GetAsync(c.Id,true,ct);
        if(notification is null||
           notification.RecipientType!=CommunicationNotificationRecipientType.User||
           notification.RecipientId!=c.UserId.Value)
            return Result.Failure(CommunicationNotificationErrors.NotFound);

        var result=notification.MarkRead(clock.UtcNow,c.UserId);
        if(result.IsFailure)return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class DismissCommunicationNotificationCommandHandler(
    ICommunicationNotificationRepository notifications,
    ICommunicationEngagementUnitOfWork uow,
    IClock clock):ICommandHandler<DismissCommunicationNotificationCommand>
{
    public async Task<Result> Handle(DismissCommunicationNotificationCommand c,CancellationToken ct)
    {
        var notification=await notifications.GetAsync(c.Id,true,ct);
        if(notification is null||
           notification.RecipientType!=CommunicationNotificationRecipientType.User||
           notification.RecipientId!=c.UserId.Value)
            return Result.Failure(CommunicationNotificationErrors.NotFound);

        var result=notification.Dismiss(clock.UtcNow,c.UserId);
        if(result.IsFailure)return result;
        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class UpdateNotificationPreferenceCommandHandler(
    INotificationPreferenceRepository preferences,
    ICommunicationEngagementUnitOfWork uow,
    IClock clock):ICommandHandler<UpdateNotificationPreferenceCommand>
{
    public async Task<Result> Handle(UpdateNotificationPreferenceCommand c,CancellationToken ct)
    {
        string category=(c.Category??string.Empty).Trim().ToUpperInvariant();
        var preference=await preferences.GetAsync(c.UserId,category,true,ct);

        if(preference is null)
        {
            var created=NotificationPreference.Create(
                new NotificationPreferenceId(Guid.NewGuid()),
                c.UserId,
                category,
                c.InAppEnabled,
                c.EmailEnabled,
                clock.UtcNow);

            if(created.IsFailure)return Result.Failure(created.Error);
            preferences.Add(created.Value);
        }
        else
        {
            var updated=preference.Update(c.InAppEnabled,c.EmailEnabled,clock.UtcNow);
            if(updated.IsFailure)return updated;
        }

        await uow.CommitAsync(ct);
        return Result.Success();
    }
}

public sealed class GetNotificationPreferencesQueryHandler(
    INotificationPreferenceRepository preferences)
    :IQueryHandler<GetNotificationPreferencesQuery,IReadOnlyList<NotificationPreferenceResponse>>
{
    public async Task<Result<IReadOnlyList<NotificationPreferenceResponse>>> Handle(
        GetNotificationPreferencesQuery q,CancellationToken ct)
    {
        var items=await preferences.ListAsync(q.UserId,ct);
        return Result.Success<IReadOnlyList<NotificationPreferenceResponse>>(
            items.Select(x=>new NotificationPreferenceResponse(
                x.Category,x.InAppEnabled,x.EmailEnabled)).ToArray());
    }
}
