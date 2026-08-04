using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;
namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.ChangeStatus;
public sealed record ChangeOrganizationSubscriptionStatusCommand(OrganizationId OrganizationId, SubscriptionStatus TargetStatus, DateTimeOffset? PeriodStartsAtUtc, DateTimeOffset? PeriodEndsAtUtc, int ExpectedVersion, string Reason, UserId ChangedByUserId) : ICommand;
