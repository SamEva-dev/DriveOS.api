namespace DriveOS.Modules.Organizations.Application.OrganizationSubscriptions.Models;

public sealed record SubscriptionEntitlementResponse(string Code);
public sealed record SubscriptionLimitResponse(string Code, long Value);
public sealed record SubscriptionPeriodResponse(DateTimeOffset StartsAtUtc, DateTimeOffset? EndsAtUtc);
public sealed record SubscriptionCancellationResponse(DateTimeOffset RequestedAtUtc, DateTimeOffset EffectiveAtUtc, string Reason, Guid RequestedByUserId);

public sealed record OrganizationSubscriptionResponse(
    Guid Id,
    Guid OrganizationId,
    string PlanCode,
    int Status,
    int BillingCycle,
    SubscriptionPeriodResponse CurrentPeriod,
    SubscriptionPeriodResponse? TrialPeriod,
    SubscriptionCancellationResponse? Cancellation,
    string? ExternalProvider,
    string? ExternalSubscriptionId,
    IReadOnlyCollection<SubscriptionEntitlementResponse> Entitlements,
    IReadOnlyCollection<SubscriptionLimitResponse> Limits,
    int Version,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastModifiedAtUtc);
