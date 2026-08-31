using DomainRelay.Abstractions;
using DriveOS.Application.Abstractions.Authentication;
using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.CommunicationEngagement.Application.Notifications;
using DriveOS.SharedKernel.Identifiers;
using DriveOS.SharedKernel.Results;

namespace DriveOS.Api.Endpoints.CommunicationEngagement;

internal static class CommunicationNotificationEndpoints
{
    internal static IEndpointRouteBuilder MapCommunicationNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var g=app.MapGroup("/api/communication/notifications")
            .WithTags("Communication - Notifications");

        g.MapGet("",List)
            .RequireAuthorization("Communication.Notifications.Read");

        g.MapGet("/unread-count",UnreadCount)
            .RequireAuthorization("Communication.Notifications.Read");

        g.MapPost("/{notificationId:guid}/read",MarkRead)
            .RequireAuthorization("Communication.Notifications.Manage");

        g.MapPost("/{notificationId:guid}/dismiss",Dismiss)
            .RequireAuthorization("Communication.Notifications.Manage");

        g.MapGet("/preferences",GetPreferences)
            .RequireAuthorization("Communication.NotificationPreferences.Manage");

        g.MapPut("/preferences/{category}",UpdatePreference)
            .RequireAuthorization("Communication.NotificationPreferences.Manage");

        return app;
    }

    private static async Task<IResult> List(
        int? take,
        bool? unreadOnly,
        ICommunicationNotificationReadService read,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var items=await read.ListForUserAsync(userId,Math.Clamp(take??50,1,200),unreadOnly??false,ct);
        return Results.Ok(items);
    }

    private static async Task<IResult> UnreadCount(
        ICommunicationNotificationReadService read,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        int count=await read.CountUnreadAsync(userId,ct);
        return Results.Ok(new{count});
    }

    private static Task<IResult> MarkRead(
        Guid notificationId,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)=>
        Mutate(currentUser,user=>new MarkCommunicationNotificationReadCommand(new(notificationId),user),mediator,ct);

    private static Task<IResult> Dismiss(
        Guid notificationId,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)=>
        Mutate(currentUser,user=>new DismissCommunicationNotificationCommand(new(notificationId),user),mediator,ct);

    private static async Task<IResult> GetPreferences(
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var result=await mediator.Send(new GetNotificationPreferencesQuery(userId),ct);
        return result.IsSuccess?Results.Ok(result.Value):Problem(result.Error);
    }

    private static async Task<IResult> UpdatePreference(
        string category,
        UpdateNotificationPreferenceRequest request,
        IMediator mediator,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        var result=await mediator.Send(new UpdateNotificationPreferenceCommand(
            userId,category,request.InAppEnabled,request.EmailEnabled),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static async Task<IResult> Mutate<T>(
        ICurrentUser currentUser,
        Func<UserId,T> factory,
        IMediator mediator,
        CancellationToken ct) where T:ICommand
    {
        if(currentUser.UserId is not{} userId)return Results.Unauthorized();
        Result result=await mediator.Send(factory(userId),ct);
        return result.IsSuccess?Results.NoContent():Problem(result.Error);
    }

    private static IResult Problem(Error e)=>e.Type switch
    {
        ErrorType.NotFound=>Results.NotFound(new{code=e.Code,messageKey=e.Message}),
        ErrorType.Conflict=>Results.Conflict(new{code=e.Code,messageKey=e.Message}),
        _=>Results.BadRequest(new{code=e.Code,messageKey=e.Message})
    };
}

internal sealed record UpdateNotificationPreferenceRequest(
    bool InAppEnabled,
    bool EmailEnabled);
