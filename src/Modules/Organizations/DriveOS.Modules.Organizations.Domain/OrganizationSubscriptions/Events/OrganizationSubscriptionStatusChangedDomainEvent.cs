using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions.Events;

public sealed record OrganizationSubscriptionStatusChangedDomainEvent(
    OrganizationSubscriptionId SubscriptionId,
    OrganizationId OrganizationId,
    SubscriptionStatus PreviousStatus,
    SubscriptionStatus CurrentStatus,
    string Reason,
    UserId ChangedByUserId) : DomainEvent;
