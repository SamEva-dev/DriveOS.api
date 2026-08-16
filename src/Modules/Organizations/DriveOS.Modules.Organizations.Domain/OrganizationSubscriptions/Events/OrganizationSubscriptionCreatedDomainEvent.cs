using DriveOS.SharedKernel.Domain;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Domain.Subscriptions.Events;

public sealed record OrganizationSubscriptionCreatedDomainEvent(
    OrganizationSubscriptionId SubscriptionId,
    OrganizationId OrganizationId,
    string PlanCode,
    SubscriptionStatus Status
) : DomainEvent;
