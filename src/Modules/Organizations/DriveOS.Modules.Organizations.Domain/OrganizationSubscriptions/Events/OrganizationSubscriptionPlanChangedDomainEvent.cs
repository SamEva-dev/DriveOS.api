using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions.Events;

public sealed record OrganizationSubscriptionPlanChangedDomainEvent(
    OrganizationSubscriptionId SubscriptionId,
    OrganizationId OrganizationId,
    string PreviousPlanCode,
    string CurrentPlanCode,
    string Reason,
    UserId ChangedByUserId) : DomainEvent;
