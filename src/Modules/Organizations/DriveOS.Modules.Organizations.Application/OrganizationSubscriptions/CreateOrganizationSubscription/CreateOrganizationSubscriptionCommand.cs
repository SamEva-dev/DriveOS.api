using DriveOS.Application.Abstractions.Messaging;
using DriveOS.Modules.Organizations.Domain.Subscriptions;
using DriveOS.SharedKernel.Identifiers;

namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.CreateOrganizationSubscription;

public sealed record CreateOrganizationSubscriptionCommand(
    OrganizationId OrganizationId,
    string PlanCode,
    SubscriptionStatus Status,
    SubscriptionBillingCycle BillingCycle,
    DateTimeOffset CurrentPeriodStartsAtUtc,
    DateTimeOffset? CurrentPeriodEndsAtUtc,
    DateTimeOffset? TrialStartsAtUtc,
    DateTimeOffset? TrialEndsAtUtc,
    string? ExternalProvider,
    string? ExternalSubscriptionId) : ICommand<OrganizationSubscriptionId>;
